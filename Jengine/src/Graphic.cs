using System.Numerics;
using Jengine.util;
using Newtonsoft.Json;
using Raylib_cs;

namespace JEngine;

public class Graphic {
    [JsonProperty] private string spritePath = "";
    
    public Vector2 Origin = Vector2.Zero;
    public Color   Colour = Color.White;
    public bool    FlipX;

    // Spritesheet
    [JsonProperty] private int cellWidth;
    [JsonProperty] private int cellHeight;
    [JsonProperty] private int cellIndex;

    // Animation
    private int  animationFrames;
    private int  animationDurationTicks;
    private int  animationStartTick;
    private bool loopAnimation = true;

    // Properties
    private Tex texture;
    public Tex Texture
    {
        get {
            if (texture == null || texture.Empty())
                texture = Find.AssetManager.GetTexture(spritePath);

            return texture;
        }
        set => texture = value;
    }

    public int CellWidth  => cellWidth  == 0 ? Texture.Width : cellWidth;
    public int CellHeight => cellHeight == 0 ? Texture.Height : cellHeight;

    [JsonConstructor]
    public Graphic() {}

    public Graphic(string path) {
        SetSprite(path);
    }
    public Graphic(string path, int cellWidth, int cellHeight) {
        SetSpritesheet(path, cellWidth, cellHeight);
    }

    public Graphic Clone() {
        return new Graphic {
            spritePath = spritePath,
            Origin     = Origin,
            Colour     = Colour,
            FlipX      = FlipX,
            cellWidth  = cellWidth,
            cellHeight = cellHeight,
            cellIndex  = cellIndex,
            animationFrames = animationFrames,
            animationDurationTicks = animationDurationTicks,
            animationStartTick = animationStartTick,
            loopAnimation = loopAnimation
        };
    }

    public void SetSprite(string path) {
        spritePath = path;
        Texture = Find.AssetManager.GetTexture(spritePath);
    }

    public void SetSpritesheet(string path, int cellWidth, int cellHeight) {
        SetSprite(path);

        if (cellWidth > Texture.Width || cellHeight > Texture.Height)
            Debug.Warn($"Spritesheet cell size is larger than texture size ({cellWidth}, {cellHeight})");

        this.cellWidth  = cellWidth;
        this.cellHeight = cellHeight;
    }

    public void SetIndex(int index) {
        cellIndex = index;
    }

    public void SetAnimation(int startIndex, int frames, int speed, bool loop = true) {
        cellIndex       = startIndex;
        animationFrames = frames;
        animationDurationTicks  = speed;
        loopAnimation   = loop;
        animationStartTick = Find.Game.Ticks;
    }

    public void Draw(
        Vector2 pos,
        float rotation = 0,
        Vector2? scale = null,
        Vector2? origin = null,
        float depth = 0,
        bool flipX = false,
        bool flipY = false,
        Color? overrideColour = null,
        int? index = null,
        int? pickId = null,
        Shader? fragShader = null,
        bool now = false
    ) {
        scale ??= Vector2.One;

        if (index == null)
            index = GetAnimationFrame();

        var source = GetCellBounds(index.Value);

        Find.Renderer.Draw(
            texture: Texture,
            pos: pos,
            scale: new Vector2(Texture.Width * source.Width, Texture.Height * source.Height) * scale,
            rotation: rotation,
            flipX: FlipX || flipX,
            flipY: flipY,
            depth: depth,
            origin: origin ?? Origin,
            source: source,
            color: overrideColour ?? Colour,
            fragShader: fragShader,
            pickId: pickId,
            now: now
        );
    }

    private int GetAnimationFrame() {
        if (animationDurationTicks == 0 || animationFrames == 0)
            return cellIndex;

        var curTick = Find.Game.Ticks - animationStartTick;

        if (!loopAnimation && curTick >= animationDurationTicks)
            return cellIndex + animationFrames - 1;

        return cellIndex + (curTick % animationDurationTicks) / (animationDurationTicks / animationFrames);
    }
    
    public Rectangle GetCellBounds() {
        return GetCellBounds(cellIndex);
    }
    
    public Rectangle GetCellBounds(int index) {
        if (Texture.Empty()) return new Rectangle(0, 0, 1, 1);

        var cols = Texture.Width / CellWidth;
        return GetCellBounds(index % cols, index / cols);
    }

    public Rectangle GetCellBounds(int col, int row) {
        if (Texture.Empty()) return new Rectangle(0, 0, 1, 1);
        
        var cols  = Texture.Width  / CellWidth;
        var rows  = Texture.Height / CellHeight;
        var xFrac = CellWidth     / (float)Texture.Width;
        var yFrac = CellHeight    / (float)Texture.Height;

        if (col >= cols || row >= rows) {
            Debug.Warn($"Spritesheet cell out of bounds ({col}, {row})");
            return new Rectangle();
        }

        return new Rectangle(
            col * xFrac,
            row * yFrac,
            xFrac,
            yFrac
        );
    }
}