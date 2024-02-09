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

    [JsonProperty] private string spritePath = "";
    [JsonProperty] private int    cellWidth  = 0;
    [JsonProperty] private int    cellHeight = 0;

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

    public void SetSprite(string path)
    {
        spritePath = path;
        Texture = Find.AssetManager.GetTexture(spritePath);
    }

    public void Blit(Vector2 pos, float rotation, Vector2 scale, float depth, Color? overrideColour = null, int index = 0, int? pickId = null, Shader? fragShader = null) {
        var source = GetCellBounds(index);

        Find.Renderer.Blit(
            texture: Texture,
            pos: pos,
            scale: new Vector2(Texture.Width * source.Width, Texture.Height * source.Height) * scale,
            rotation: rotation,
            depth: depth,
            origin: origin,
            source: source,
            color: overrideColour ?? colour,
            fragShader: fragShader,
            pickId: pickId
        );
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