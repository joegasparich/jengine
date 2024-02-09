using Raylib_cs;

namespace JEngine.util;

public class CachedTexture {
    private string path;

    public Texture2D Texture => Find.AssetManager.GetTexture(path);

    public CachedTexture(string path) {
        this.path = path;
    }
}