using Raylib_cs;

namespace JEngine.util;

public class Gradient {
    public (float, Color)[] colours;
    
    public Gradient(params (float, Color)[] colours) {
        this.colours = colours;
    }
    
    public Color Calculate(float value) {
        for (var i = 0; i < colours.Length - 1; i++) {
            if (value >= colours[i].Item1 && value <= colours[i + 1].Item1) {
                var t = (value - colours[i].Item1) / (colours[i + 1].Item1 - colours[i].Item1);
                return Colour.Lerp(colours[i].Item2, colours[i + 1].Item2, t);
            }
        }
        
        return colours[^1].Item2;
    }
}