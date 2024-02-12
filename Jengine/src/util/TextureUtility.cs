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

    public static float ScaleToFit(int width, int height, int maxWidth, int maxHeight)
    {
        var widthScale  = maxWidth / (float)width;
        var heightScale = maxHeight / (float)height;

        return Math.Min(widthScale, heightScale);
    }
}