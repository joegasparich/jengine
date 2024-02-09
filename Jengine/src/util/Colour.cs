using System.Numerics;
using Raylib_cs;

namespace JEngine.util; 

public static class Colour {
    public static readonly Color Transparent = new(0, 0, 0, 0);
    public static readonly Color Primary = new(77, 134, 179, 255);
    public static readonly Color Cancel  = new(209, 84, 84, 255);
    public static readonly Color White   = new(255, 255, 255, 255);
    public static readonly Color Grey    = new(170, 170, 170, 255);

    public static Color IntToColour(int number) {
        return new Color(
            (number >>  0) & 255,
            (number >>  8) & 255,
            (number >> 16) & 255,
            255
        );
    }

    public static int ColourToInt(Color colour) {
        return ( colour.R << 0 ) | ( colour.G << 8 ) | ( colour.B << 16 );
    }
    
    public static Vector3 ToVector3(this Color color) {
        return new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
    }

    public static Color FromVector3(Vector3 vec) {
        return new Color((byte)(vec.X * 255), (byte)(vec.Y * 255), (byte)(vec.Z * 255), (byte)255);
    }
    
    public static Color Brighten(this Color color, float amount) {
        var (h, s, v) = Raylib.ColorToHSV(color);
        v = JMath.Clamp(v + amount, 0.0f, 1.0f);
        return Raylib.ColorFromHSV(h, s, v);
    }

    public static Color WithAlpha(this Color color, float alpha) {
        return new Color(color.R, color.G, color.B, (byte)(alpha * 255));
    }
}