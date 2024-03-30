using Jengine.util;

namespace JEngine.util;

public class CachedTexture {
    private string path;

    public Tex Texture => Find.AssetManager.GetTexture(path);

    public CachedTexture(string path) {
        this.path = path;
    }
}