using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public class Graphic {
    [JsonProperty] private string _spritePath = "";
    
    public Vector2 Origin = Vector2.Zero;
    public Color   Colour = Color.White;
    public bool    FlipX;

    // Spritesheet
    [JsonProperty] private int _cellWidth;
    [JsonProperty] private int _cellHeight;
    private                int _cellIndex;

    // Animation
    private int  _animationFrames;
    private int  _animationDurationTicks;
    private int  _animationStartTick;
    private bool _loopAnimation = true;

    // Properties
    private Texture2D _texture;
    public Texture2D Texture
    {
        get {
            if (_texture.Empty())
                _texture = Find.AssetManager.GetTexture(_spritePath);

            return _texture;
        }
        set => _texture = value;
    }

    public int CellWidth  => _cellWidth  == 0 ? Texture.Width : _cellWidth;
    public int CellHeight => _cellHeight == 0 ? Texture.Height : _cellHeight;

    [JsonConstructor]
    public Graphic() {}

    public Graphic(string path) {
        SetSprite(path);
    }
    public Graphic(string path, int cellWidth, int cellHeight) {
        SetSpritesheet(path, cellWidth, cellHeight);
    }

    public void SetSprite(string path) {
        _spritePath = path;
        Texture = Find.AssetManager.GetTexture(_spritePath);
    }

    public void SetSpritesheet(string path, int cellWidth, int cellHeight) {
        SetSprite(path);

        if (cellWidth > Texture.Width || cellHeight > Texture.Height)
            Debug.Warn($"Spritesheet cell size is larger than texture size ({cellWidth}, {cellHeight})");

        this._cellWidth  = cellWidth;
        this._cellHeight = cellHeight;
    }

    public void SetIndex(int index) {
        _cellIndex = index;
    }

    public void SetAnimation(int startIndex, int frames, int speed, bool loop = true) {
        _cellIndex       = startIndex;
        _animationFrames = frames;
        _animationDurationTicks  = speed;
        _loopAnimation   = loop;
        _animationStartTick = Find.Game.Ticks;
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
            flipX: this.FlipX || flipX,
            flipY: flipY,
            depth: depth,
            origin: Origin,
            source: source,
            color: overrideColour ?? Colour,
            fragShader: fragShader,
            pickId: pickId,
            now: now
        );
    }

    private int GetAnimationFrame() {
        if (_animationDurationTicks == 0 || _animationFrames == 0)
            return _cellIndex;

        var curTick = Find.Game.Ticks - _animationStartTick;

        if (!_loopAnimation && curTick >= _animationDurationTicks)
            return _cellIndex + _animationFrames - 1;

        return _cellIndex + (curTick % _animationDurationTicks) / (_animationDurationTicks / _animationFrames);
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