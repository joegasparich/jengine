using Jengine.util;
using Raylib_cs;

namespace JEngine.util;

public static class TextureUtility {
    public static IntVec2 MaintainAspectRatio(int width, int height, int maxWidth, int maxHeight) {
        var aspectRatio = width / (float)height;

        if (width > maxWidth)
            return new IntVec2(maxWidth, (int)(width / aspectRatio));
        if (height > maxHeight)
            return new IntVec2((int)(height * aspectRatio), maxHeight);

        return new IntVec2(width, height);
    }

    public static Rectangle MaintainAspectRatio(Rectangle rect, Tex texture, Rectangle? source = null) {
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

    public static float ScaleToFit(int width, int height, int maxWidth, int maxHeight)
    {
        var widthScale  = maxWidth / (float)width;
        var heightScale = maxHeight / (float)height;

        return Math.Min(widthScale, heightScale);
    }
}