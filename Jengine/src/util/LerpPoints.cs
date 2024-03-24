namespace JEngine.util;

public class LerpPoints {
    public List<(float, float)> Points;

    public LerpPoints(IEnumerable<(float, float)> points) {
        Points = points.ToList();
    }
    
    public LerpPoints(params (float, float)[] points) {
        Points = points.ToList();
    }

    public float Calculate(float inVal) {
        if (Points.Count == 0)
            return 0;

        if (Points.Count == 1)
            return Points[0].Item2;
        
        for (var i = 0; i < Points.Count - 1; i++) {
            if (inVal >= Points[i].Item1 && inVal <= Points[i + 1].Item1) {
                var t = (inVal - Points[i].Item1) / (Points[i + 1].Item1 - Points[i].Item1);
                return JMath.Lerp(Points[i].Item2, Points[i + 1].Item2, t);
            }
        }
        
        return Points[^1].Item2;
    }
}