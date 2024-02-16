using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public class GraphicData {
    [JsonProperty] private string spritePath = "";
    
    public Vector2 origin = Vector2.Zero;
    public Color   colour = Color.White;
    public bool    flipX;

    // Spritesheet
    [JsonProperty] private int cellWidth;
    [JsonProperty] private int cellHeight;
    private                int cellIndex;

    // Animation
    private int animationFrames;
    private int animationSpeed;

    // Properties
    private Texture2D texture;
    public Texture2D Texture
    {
        get {
            if (texture.Empty())
                texture = Find.AssetManager.GetTexture(spritePath);

            return texture;
        }
        set => texture = value;
    }

    public int CellWidth  => cellWidth  == 0 ? Texture.Width : cellWidth;
    public int CellHeight => cellHeight == 0 ? Texture.Height : cellHeight;

    [JsonConstructor]
    public GraphicData() {}

    public GraphicData(string path) {
        SetSprite(path);
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

    public void SetAnimation(int startIndex, int frames, int speed) {
        cellIndex       = startIndex;
        animationFrames = frames;
        animationSpeed  = speed;
    }

    public void Draw(
        Vector2 pos,
        float rotation = 0,
        Vector2? scale = null,
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
            flipX: this.flipX || flipX,
            flipY: flipY,
            depth: depth,
            origin: origin,
            source: source,
            color: overrideColour ?? colour,
            fragShader: fragShader,
            pickId: pickId,
            now: now
        );
    }

    private int GetAnimationFrame() {
        if (animationSpeed == 0 || animationFrames == 0)
            return cellIndex;

        return cellIndex + Find.Game.Ticks % animationSpeed / (animationSpeed / animationFrames);
    }
    
    public Rectangle GetCellBounds(int cellIndex) {
        if (Texture.Empty()) return new Rectangle(0, 0, 1, 1);

        var cols = Texture.Width / CellWidth;
        return GetCellBounds(cellIndex % cols, cellIndex / cols);
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