using System.Collections.Concurrent;
using System.Numerics;
using JEngine.util;
using Raylib_cs;

namespace JEngine;

public enum Depth
{
    Ground = -1,
    Overlay = -4,
    YSorting = - 5,
    Debug = -8,
    UI = -9,
    Camera = -10
}

internal class Blit {
    public Texture2D texture;
    public Rectangle sourceRect; 
    public Rectangle destRect; 
    public Vector3   origin; 
    public float     rotation; 
    public float     posZ;
    public Color     tint;
    public Shader?   fragShader;
    public int?      pickId;
}

public class Renderer {
    // Resources
    public static  readonly Shader OutlineShader      = Raylib.LoadShader(null, "assets/shaders/outline.fsh");
    private static readonly Shader DiscardAlphaShader = Raylib.LoadShader(null, "assets/shaders/discard_alpha.fsh");
    private static readonly Shader PickShader         = Raylib.LoadShader(null, "assets/shaders/pick.fsh");
    private static readonly int    PickColourLoc      = Raylib.GetShaderLocation(PickShader, "pickColor");
    
    // Collections
    private ConcurrentQueue<Blit> blits = new();
    
    // State
    public  Camera          camera = new();
    private RenderTexture2D pickBuffer;
    private Image           pickImage;
    
    public Renderer() {
        Debug.Log("Initialising Renderer");
        Raylib.SetTargetFPS(60);
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

        pickBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        
        // Set outline colour
        var outlineColLoc = Raylib.GetShaderLocation(OutlineShader, "outlineCol");
        Raylib.SetShaderValue(OutlineShader, outlineColLoc, new Vector4(0.4f, 0.7f, 1f, 1f), ShaderUniformDataType.Vec4);
        
        Rlgl.EnableDepthTest();
    }

    public void Update() {}

    public void Render() {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Gray);
        
        Raylib.BeginMode3D(camera.Cam);
        {
            Find.Game.Render();
            
            Raylib.BeginShaderMode(DiscardAlphaShader);
            foreach (var blit in blits) {
                DoBlit(blit);
            }
            Raylib.EndShaderMode();
            
            Find.Game.RenderLate();
        }
        Raylib.EndMode3D();
        
        RenderPickIdsToBuffer();
        Raylib.UnloadImage(pickImage);
        pickImage = Raylib.LoadImageFromTexture(pickBuffer.Texture);
        // if (DebugSettings.Get(DebugSetting.DrawPickBuffer))
        //     RenderPickBuffer();
        
        Find.Game.Render2D();
        Raylib.EndDrawing();
        
        blits.Clear();
    }

    public void OnScreenResized() {
        Raylib.UnloadRenderTexture(pickBuffer);
        pickBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        
        camera.OnScreenResized();
    }

    public float GetDepth(float yPos) {
        return Math.Clamp(yPos / Game.LargerThanWorld, 0, 1) * -1 + (int)Depth.YSorting;
    }

    public void Blit(
        Texture2D  texture,
        Vector2    pos,
        float      depth  = 0,
        Vector2?   scale  = null,
        Vector2?   origin = null,
        Rectangle? source = null,
        float      rotation = 0,
        Color?     color  = null,
        Shader?    fragShader = null,
        int?       pickId = null
    ) {
        scale  ??= new Vector2(texture.Width, texture.Height);
        origin ??= new Vector2(0, 0);
        source ??= new Rectangle(0, 0, 1, 1);
        color  ??= Color.White;
        
        // Cull offscreen blits
        if (!IsPosOnScreen(pos, MathF.Max(scale.Value.X, scale.Value.Y))) 
            return;
        
        var src = new Rectangle(
            source.Value.X      * texture.Width,
            source.Value.Y      * texture.Height,
            source.Value.Width  * texture.Width,
            source.Value.Height * texture.Height
        );
        var scaledOrigin = origin.Value * scale.Value;

        blits.Enqueue(new Blit {
            texture    = texture,
            sourceRect = src,
            destRect   = new Rectangle(pos.X, pos.Y, scale.Value.X, scale.Value.Y),
            origin     = new Vector3(scaledOrigin.X, scaledOrigin.Y, 0),
            rotation   = rotation,
            posZ       = depth,
            tint       = color.Value,
            fragShader = fragShader,
            pickId     = pickId
        });
    }

    private void DoBlit(Blit blit, bool picking = false) {
        // TODO: Look into whether switching shaders is expensive
        if (!picking && blit.fragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(blit.fragShader.Value);
        }
            
        Draw.DrawTexturePro3D(
            blit.texture,
            blit.sourceRect,
            blit.destRect,
            blit.origin,
            blit.rotation,
            blit.posZ,
            blit.tint
        );
        
        if (!picking && blit.fragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(DiscardAlphaShader);
        }
    }
    
    // Picking //

    private void RenderPickIdsToBuffer() {
        Raylib.BeginTextureMode(pickBuffer);
        Raylib.ClearBackground(Color.White);
        Raylib.BeginMode3D(camera.Cam);
        {
            foreach (var blit in blits) {
                if (!blit.pickId.HasValue) 
                    continue;
                
                Raylib.BeginShaderMode(PickShader);
                Raylib.SetShaderValue(PickShader, PickColourLoc, Colour.IntToColour(blit.pickId.Value).ToVector3(), ShaderUniformDataType.Vec3);
                DoBlit(blit, true);
                Raylib.EndShaderMode();
            }
        }
        Raylib.EndMode3D();
        Raylib.EndTextureMode();
    }

    private void RenderPickBuffer() {
        Raylib.DrawTexturePro(
            pickBuffer.Texture,
            new Rectangle(0, 0, pickBuffer.Texture.Width, -pickBuffer.Texture.Height),
            new Rectangle(0, Find.Game.ScreenHeight - 112, 173, 112),
            new Vector2(0, 0),
            0,
            Color.White
        );
    }
    
    public int GetPickIdAtPos(Vector2 screenPos) {
        var pixel = Raylib.GetImageColor(pickImage, screenPos.X.FloorToInt(), Find.Game.ScreenHeight - screenPos.Y.FloorToInt());
        if (pixel.Equals(Color.White)) 
            return -1;
        
        return Colour.ColourToInt(pixel);
    }
    
    // Utility //
    
    public Vector2 ScreenToWorldPos(Vector2 screenPos) {
        var cameraCenter = (camera.Position * camera.Zoom) - new Vector2(Find.Game.ScreenWidth/2f, Find.Game.ScreenHeight/2f);
        return (screenPos + cameraCenter) / (Game.WorldScale * camera.Zoom);
    }
    
    // TODO: make sure this is correct since this is literally AI generated
    public Vector2 WorldToScreenPos(Vector2 worldPos) {
        return worldPos * (Game.WorldScale * camera.Zoom) - (camera.Position * camera.Zoom) + new Vector2(Find.Game.ScreenWidth/2f, Find.Game.ScreenHeight/2f);
    }

    public bool IsPosOnScreen(Vector2 pos, float margin = 0) {
        return pos.X > camera.Position.X - Find.Game.ScreenWidth/2f  - margin && pos.X < camera.Position.X + Find.Game.ScreenWidth/2f + margin
            && pos.Y > camera.Position.Y - Find.Game.ScreenHeight/2f - margin && pos.Y < camera.Position.Y + Find.Game.ScreenHeight/2f + margin;
    }

    public bool IsWorldPosOnScreen(Vector2 worldPos, float margin = 32) {
        var topLeft = ScreenToWorldPos(new Vector2(0, 0) - new Vector2(margin, margin));
        var bottomRight = ScreenToWorldPos(new Vector2(Find.Game.ScreenWidth, Find.Game.ScreenHeight) + new Vector2(margin, margin));
        
        return worldPos.X > topLeft.X && worldPos.X < bottomRight.X 
            && worldPos.Y > topLeft.Y && worldPos.Y < bottomRight.Y;
    }

    public bool IsWorldRectOnScreen(Rectangle rect, float margin = 32) {
        var topLeft     = ScreenToWorldPos(new Vector2(0, 0) - new Vector2(margin, margin));
        var bottomRight = ScreenToWorldPos(new Vector2(Find.Game.ScreenWidth, Find.Game.ScreenHeight) + new Vector2(margin, margin));

        return rect.X + rect.Width > topLeft.X     &&
            rect.X                 < bottomRight.X &&
            rect.Y + rect.Height   > topLeft.Y     &&
            rect.Y                 < bottomRight.Y;
    }
}