using Raylib_cs;

namespace JEngine.util;

public static class TextureUtility {
    public static Rectangle MaintainAspectRatio(Rectangle rect, Texture2D texture, Rectangle? source = null) {
        source ??= new Rectangle(0, 0, 1, 1);

        var texWidth  = texture.Width * source.Value.Width;
        var texHeight = texture.Height * source.Value.Height;

        if (texWidth < texHeight) {
            var width = rect.Width * (texWidth / texHeight);
            return new Rectangle(rect.X + (rect.Width - width) / 2, rect.Y, width, rect.Height);
        }
        if (texWidth > texHeight) {
            var height = rect.Height * (texHeight / texWidth);
            return new Rectangle(rect.X, rect.Y + (rect.Height - height) / 2, rect.Width, height);
        }

        return rect;
    }
}