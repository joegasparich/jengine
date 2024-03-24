using Raylib_cs;

namespace JEngine.util;

public class CachedTexture {
    private string _path;

    public Texture2D Texture => Find.AssetManager.GetTexture(_path);

    public CachedTexture(string path) {
        _path = path;
    }
}