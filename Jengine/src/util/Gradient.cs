using Raylib_cs;

namespace JEngine.util;

public class Gradient {
    public (float, Color)[] Colours;
    
    public Gradient(params (float, Color)[] colours) {
        Colours = colours;
    }
    
    public Color Calculate(float value) {
        for (var i = 0; i < Colours.Length - 1; i++) {
            if (value >= Colours[i].Item1 && value <= Colours[i + 1].Item1) {
                var t = (value - Colours[i].Item1) / (Colours[i + 1].Item1 - Colours[i].Item1);
                return Colour.Lerp(Colours[i].Item2, Colours[i + 1].Item2, t);
            }
        }
        
        return Colours[^1].Item2;
    }
}