using System.Numerics;
using JEngine.util;
using Raylib_cs;

namespace JEngine;

public enum Depth
{
    Ground = -1,
    Below = -3,
    YSorting = -5,
    Above = -7,
    Debug = -8,
    Ui = -9,
    Camera = -10
}

public class RendererConfig {
    public Vector2? FixedResolution;
}

internal class DrawCall {
    public Texture2D Texture;
    public Rectangle SourceRect; 
    public Rectangle DestRect; 
    public Vector2   Origin;
    public float     Rotation; 
    public float     PosZ;
    public Color     Tint;
    public Shader?   FragShader;
    public int?      PickId;
}

public class Renderer {
    // Resources
    private static readonly Shader outlineShader      = Raylib.LoadShader(null, "assets/shaders/outline.fsh");
    private static readonly Shader basicShader        = Raylib.LoadShader(null, "assets/shaders/basic.fsh");
    private static readonly Shader discardAlphaShader = Raylib.LoadShader(null, "assets/shaders/discard_alpha.fsh");
    private static readonly Shader pickShader         = Raylib.LoadShader(null, "assets/shaders/pick.fsh");
    private static readonly int    pickColourLoc      = Raylib.GetShaderLocation(pickShader, "pickColor");

    public RendererConfig RenderConfig = new();
    
    // Collections
    private SortedList<float, DrawCall> drawCalls = new(new DuplicateKeyComparer<float>());
    
    // State
    public  Camera          Camera;
    private RenderTexture2D screenBuffer;
    private RenderTexture2D pickBuffer;
    private Image           pickImage;
    private bool            drawingWorld;

    public void Init() {
        Debug.Log("Initialising Renderer");
        Raylib.SetTargetFPS(60);
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

        Camera = new();

        screenBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        pickBuffer = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);

        // Set outline colour
        var outlineColLoc = Raylib.GetShaderLocation(outlineShader, "outlineCol");
        Raylib.SetShaderValue(outlineShader, outlineColLoc, new Vector4(0.4f, 0.7f, 1f, 1f), ShaderUniformDataType.Vec4);

        // Rlgl.EnableDepthTest();
        Rlgl.EnableColorBlend();
        Rlgl.SetBlendMode(BlendMode.Alpha);
    }

    public void Update() {
        // Debug.Log(Find.Input.GetMouseWorldPos().ToString());
    }

    public void Draw() {
        Raylib.BeginDrawing();
        {
            Raylib.BeginTextureMode(screenBuffer);
            {
                Raylib.ClearBackground(Color.Black);

                Raylib.BeginMode3D(Camera.Cam);
                {
                    Raylib.BeginShaderMode(discardAlphaShader);
                    drawingWorld = true;
                    Find.Game.Draw();

                    foreach (var drawCall in drawCalls) {
                        DrawNow(drawCall.Value);
                    }

                    Find.Game.DrawLate();
                    drawingWorld = false;
                    Raylib.EndShaderMode();
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

        Camera.OnScreenResized();
    }

    public float GetDepth(float yPos) {
        var posRelToCam = yPos - Camera.WorldPos.Y;
        return Math.Clamp(posRelToCam / 10000, 0, 1) * -1 + (int)Depth.YSorting;
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

        // Cull offscreen draw calls
        if (!now && !IsWorldPosOnScreen(pos, MathF.Max(scale.Value.X, scale.Value.Y)))
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
            Texture = texture,
            SourceRect = src,
            DestRect = new Rectangle(pos.X, pos.Y, scale.Value.X, scale.Value.Y),
            Origin = new Vector2(scaledOrigin.X, scaledOrigin.Y),
            Rotation = rotation,
            PosZ = depth,
            Tint = color.Value,
            FragShader = fragShader,
            PickId = pickId
        };

        if (now) {
            DrawNow(call);
        } else {
            drawCalls.Add(-depth, call);
        }
    }

    private void DrawNow(DrawCall drawCall, bool picking = false) {
        // TODO: Look into whether switching shaders is expensive
        if (!picking && drawCall.FragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(drawCall.FragShader.Value);
        }

        if (drawingWorld)
            drawCall.DestRect.Position *= Find.Config.WorldScalePx;
            
        Drawing.DrawTexture(
            drawCall.Texture,
            drawCall.SourceRect,
            drawCall.DestRect,
            drawCall.Origin,
            drawCall.Rotation,
            drawCall.PosZ,
            drawCall.Tint
        );
        
        if (!picking && drawCall.FragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(discardAlphaShader);
        }
    }
    
    // Picking //

    private void RenderPickIdsToBuffer() {
        Raylib.BeginTextureMode(pickBuffer);
        Raylib.ClearBackground(Color.White);
        Raylib.BeginMode3D(Camera.Cam);
        {
            foreach (var drawCall in drawCalls) {
                if (!drawCall.Value.PickId.HasValue)
                    continue;

                Raylib.BeginShaderMode(pickShader);
                Raylib.SetShaderValue(pickShader, pickColourLoc, Colour.IntToColour(drawCall.Value.PickId.Value).ToVector3(), ShaderUniformDataType.Vec3);
                DrawNow(drawCall.Value, true);
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
        var cameraCenter = (Camera.Position * Camera.Zoom) - new Vector2(Find.Game.ScreenWidth/2f, Find.Game.ScreenHeight/2f);
        return (screenPos + cameraCenter) / (Find.Config.WorldScalePx * Camera.Zoom);
    }

    public Vector2 WorldToScreenPos(Vector2 worldPos) {
        var cameraCenter = (Camera.Position * Camera.Zoom) - new Vector2(Find.Game.ScreenWidth / 2f, Find.Game.ScreenHeight / 2f);
        return (worldPos * (Find.Config.WorldScalePx * Camera.Zoom)) - cameraCenter;
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