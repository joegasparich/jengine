using System.Numerics;
using Raylib_cs;

namespace JEngine.util; 

public static class RaylibExtension {
    // Texture2D //
    public static bool Empty(this Texture2D tex) {
        return tex.Id == 0;
    }
    public static Vector2 Dimensions(this Texture2D tex) {
        return new(tex.Width, tex.Height);
    }

    // Key //
    public static bool IsAlphanumeric(this KeyboardKey key) {
        return key is >= KeyboardKey.Apostrophe and <= KeyboardKey.Grave;
    }
    
    // Color //
    // extension method to add two colors
    public static int ToInt(this Color c) {
        return Colour.ColourToInt(c);
    }
}