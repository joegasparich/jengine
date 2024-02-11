using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public struct GraphicData {
    // Config
    public  Vector2   origin = new(0.5f);
    public  Color     colour = Color.White;
    private Texture2D texture;

    [JsonProperty] private string spritePath      = "";
    [JsonProperty] private int    cellWidth       = 0;
    [JsonProperty] private int    cellHeight      = 0;
    private                int    cellIndex       = 0;

    public bool flipX = false;

    private int animationFrames = 0;
    private int animationSpeed  = 0;

    // Properties
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

    public void Blit(Vector2 pos, float rotation, Vector2 scale, float depth, Color? overrideColour = null, int? index = null, int? pickId = null, Shader? fragShader = null) {
        if (index == null)
            index = GetAnimationFrame();

        var source = GetCellBounds(index.Value);

        Find.Renderer.Blit(
            texture: Texture,
            pos: pos,
            scale: new Vector2(Texture.Width * source.Width, Texture.Height * source.Height) * scale,
            rotation: rotation,
            flipX: flipX,
            depth: depth,
            origin: origin,
            source: source,
            color: overrideColour ?? colour,
            fragShader: fragShader,
            pickId: pickId
        );
    }

    private int GetAnimationFrame() {
        if (animationSpeed == 0 || animationFrames == 0)
            return cellIndex;

        return cellIndex + Find.Game.Ticks % animationSpeed / (animationSpeed / animationFrames);
    }
    
    public Rectangle GetCellBounds(int cellIndex) {
        if (Texture.Empty()) return new Rectangle(0, 0, 1, 1);
        
        var cols = Texture.Width        / CellWidth;
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