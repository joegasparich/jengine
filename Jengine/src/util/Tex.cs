using System.Collections.Concurrent;
using System.Numerics;
using JEngine;
using Raylib_cs;

namespace Jengine.util;

public class Tex : Disposable {
    private Texture2D texture;
    private string    path;

    public virtual    uint      Id      => texture.Id;
    public virtual    int       Width   => texture.Width;
    public virtual    int       Height  => texture.Height;
    protected virtual Texture2D Texture => texture;
    
    public Tex() {}
    public Tex(string path) {
        texture = Raylib.LoadTexture(path);
        this.path = path;
    }
    
    public static implicit operator Texture2D(Tex texture) => texture.Texture;

    public virtual Vector2 Dimensions => new(texture.Width, texture.Height);
    public virtual bool    Empty()    => texture.Id == 0;
    
    protected override void DisposeUnmanagedObjects() {
        // We can't directly unload here because we are on the GC thread
        Find.AssetManager.Unload(texture);
    }
}

public class RenderTex : Tex {
    private RenderTexture2D renderTex;

    public override    uint      Id      => renderTex.Id;
    public override    int       Width   => renderTex.Texture.Width;
    public override    int       Height  => renderTex.Texture.Height;
    protected override Texture2D Texture => renderTex.Texture;
    
    public RenderTex(int width, int height) {
        renderTex = Raylib.LoadRenderTexture(width, height);
    }
    
    public static implicit operator RenderTexture2D(RenderTex texture) => texture.renderTex;

    public override Vector2 Dimensions => new(renderTex.Texture.Width, renderTex.Texture.Height);
    public override bool    Empty()    => renderTex.Id == 0 || renderTex.Texture.Id == 0;

    protected override void DisposeUnmanagedObjects() {
        // We can't directly unload here because we are on the GC thread
        Find.AssetManager.Unload(renderTex);
    }
}