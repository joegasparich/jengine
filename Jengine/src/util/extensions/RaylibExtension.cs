using System.Numerics;
using Raylib_cs;

namespace JEngine.util; 

public static class RaylibExtension {
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