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

public class RendererConfig {
    public Vector2? fixedResolution;
}

internal class DrawCall {
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
    private static readonly Shader OutlineShader      = Raylib.LoadShader(null, "assets/shaders/outline.fsh");
    private static readonly Shader DiscardAlphaShader = Raylib.LoadShader(null, "assets/shaders/discard_alpha.fsh");
    private static readonly Shader PickShader         = Raylib.LoadShader(null, "assets/shaders/pick.fsh");
    private static readonly int    PickColourLoc      = Raylib.GetShaderLocation(PickShader, "pickColor");

    public RendererConfig renderConfig = new();
    
    // Collections
    private List<DrawCall> drawCalls = new();
    
    // State
    public  Camera          camera;
    private RenderTexture2D screenBuffer;
    private RenderTexture2D pickBuffer;
    private Image           pickImage;
    private bool            drawingWorld;

    public void Init() {
        Debug.Log("Initialising Renderer");
        Raylib.SetTargetFPS(60);
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

        camera = new();

        screenBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        pickBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);

        // Set outline colour
        var outlineColLoc = Raylib.GetShaderLocation(OutlineShader, "outlineCol");
        Raylib.SetShaderValue(OutlineShader, outlineColLoc, new Vector4(0.4f, 0.7f, 1f, 1f), ShaderUniformDataType.Vec4);

        Rlgl.EnableDepthTest();
    }

    public void Update() {
        // Debug.Log(Find.Input.GetMouseWorldPos().ToString());
    }

    public void Draw() {
        Raylib.BeginDrawing();
        {
            Raylib.BeginTextureMode(screenBuffer);
            {
                Raylib.ClearBackground(Color.Gray);

                Raylib.BeginMode3D(camera.Cam);
                {
                    drawingWorld = true;
                    Find.Game.Draw();

                    Raylib.BeginShaderMode(DiscardAlphaShader);
                    foreach (var drawCall in drawCalls) {
                        DrawNow(drawCall);
                    }
                    Raylib.EndShaderMode();

                    Find.Game.RenderLate();
                    drawingWorld = false;
                }
                Raylib.EndMode3D();

                RenderPickIdsToBuffer();
                Raylib.UnloadImage(pickImage);
                pickImage = Raylib.LoadImageFromTexture(pickBuffer.Texture);

            }
            Raylib.EndTextureMode();

            Raylib.DrawTexturePro(
                screenBuffer.Texture,
                new Rectangle(0, 0, Find.Game.ScreenWidth, -Find.Game.ScreenHeight),
                new Rectangle(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight),
                new Vector2(0, 0),
                0,
                Color.White
            );
        }

        Find.Game.DrawUI();
        
        // RenderPickBuffer();

        Raylib.EndDrawing();

        drawCalls.Clear();
    }

    public void OnScreenResized() {
        Raylib.UnloadRenderTexture(pickBuffer);
        Raylib.UnloadRenderTexture(screenBuffer);
        pickBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        screenBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);

        camera.OnScreenResized();
    }

    public float GetDepth(float yPos) {
        return Math.Clamp(yPos / Game.LargerThanWorld, 0, 1) * -1 + (int)Depth.YSorting;
    }

    public void Draw(
        Texture2D  texture,
        Vector2    pos,
        float      depth  = 0,
        Vector2?   scale  = null,
        Vector2?   origin = null,
        Rectangle? source = null,
        float      rotation = 0,
        Color?     color  = null,
        Shader?    fragShader = null,
        int?       pickId = null,
        bool       flipX = false,
        bool       flipY = false,
        bool       now = false
    ) {
        scale  ??= new Vector2(texture.Width, texture.Height);
        origin ??= new Vector2(0, 0);
        source ??= new Rectangle(0, 0, 1, 1);
        color  ??= Color.White;

        bool IsPosOnScreen(Vector2 pos, float margin) => 
            pos.X > camera.Position.X - Find.Game.ScreenWidth / 2f - margin && pos.X < camera.Position.X + Find.Game.ScreenWidth / 2f + margin && 
            pos.Y > camera.Position.Y - Find.Game.ScreenHeight / 2f - margin && pos.Y < camera.Position.Y + Find.Game.ScreenHeight / 2f + margin;

        // Cull offscreen draw calls
        if (!now && !IsPosOnScreen(pos, MathF.Max(scale.Value.X, scale.Value.Y)))
            return;

        var src = new Rectangle(
            source.Value.X      * texture.Width,
            source.Value.Y      * texture.Height,
            source.Value.Width  * texture.Width * (flipX ? -1 : 1),
            source.Value.Height * texture.Height * (flipY ? -1 : 1)
        );
        var scaledOrigin = origin.Value * scale.Value;

        var call = new DrawCall
        {
            texture = texture,
            sourceRect = src,
            destRect = new Rectangle(pos.X, pos.Y, scale.Value.X, scale.Value.Y),
            origin = new Vector3(scaledOrigin.X, scaledOrigin.Y, 0),
            rotation = rotation,
            posZ = depth,
            tint = color.Value,
            fragShader = fragShader,
            pickId = pickId
        };

        if (now) {
            DrawNow(call);
        } else {
            drawCalls.Add(call);
        }
    }

    private void DrawNow(DrawCall drawCall, bool picking = false) {
        // TODO: Look into whether switching shaders is expensive
        if (!picking && drawCall.fragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(drawCall.fragShader.Value);
        }

        if (drawingWorld)
            drawCall.destRect.Position *= Find.Config.worldScalePx;
            
        util.Draw.DrawTexture(
            drawCall.texture,
            drawCall.sourceRect,
            drawCall.destRect,
            drawCall.origin,
            drawCall.rotation,
            drawCall.posZ,
            drawCall.tint
        );
        
        if (!picking && drawCall.fragShader.HasValue) {
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
            foreach (var drawCall in drawCalls) {
                if (!drawCall.pickId.HasValue)
                    continue;

                Raylib.BeginShaderMode(PickShader);
                Raylib.SetShaderValue(PickShader, PickColourLoc, Colour.IntToColour(drawCall.pickId.Value).ToVector3(), ShaderUniformDataType.Vec3);
                DrawNow(drawCall, true);
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
        if (!InScreenBounds(screenPos))
            return -1;
        
        var pixel = Raylib.GetImageColor(pickImage, screenPos.X.FloorToInt(), Find.Game.ScreenHeight - screenPos.Y.FloorToInt());
        if (pixel.Equals(Color.White)) 
            return -1;
        
        return Colour.ColourToInt(pixel);
    }
    
    // Utility //
    
    public Vector2 ScreenToWorldPos(Vector2 screenPos) {
        var cameraCenter = (camera.Position * camera.zoom) - new Vector2(Find.Game.ScreenWidth/2f, Find.Game.ScreenHeight/2f);
        return (screenPos + cameraCenter) / (Find.Config.worldScalePx * camera.zoom);
    }

    public Vector2 WorldToScreenPos(Vector2 worldPos) {
        var cameraCenter = (camera.Position * camera.zoom) - new Vector2(Find.Game.ScreenWidth / 2f, Find.Game.ScreenHeight / 2f);
        return (worldPos * (Find.Config.worldScalePx * camera.zoom)) - cameraCenter;
    }
    
    public bool InScreenBounds(Vector2 screenPos) {
        return screenPos.X > 0 && screenPos.X < Find.Game.ScreenWidth && screenPos.Y > 0 && screenPos.Y < Find.Game.ScreenHeight;
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