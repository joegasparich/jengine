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
        
        var texAspectRatio  = texWidth / texHeight;
        var rectAspectRatio = rect.Width / rect.Height;

        if (texAspectRatio > rectAspectRatio) {
            // Adjust height to maintain aspect ratio
            var newHeight  = (int)Math.Round(rect.Width / texAspectRatio);
            var heightDiff = rect.Height - newHeight;
            return new Rectangle(rect.X, rect.Y + heightDiff / 2, rect.Width, newHeight);
        } else {
            // Adjust width to maintain aspect ratio
            var newWidth  = (int)Math.Round(rect.Height * texAspectRatio);
            var widthDiff = rect.Width - newWidth;
            return new Rectangle(rect.X + widthDiff / 2, rect.Y, newWidth, rect.Height);
        }
    }

    public static float ScaleToFit(int width, int height, int maxWidth, int maxHeight)
    {
        var widthScale  = maxWidth / (float)width;
        var heightScale = maxHeight / (float)height;

        return Math.Min(widthScale, heightScale);
    }
}