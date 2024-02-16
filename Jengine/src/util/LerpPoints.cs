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
        
        // // Beyond left limit
        // if (inVal < Points[0].Item1 && Points[0].Item1 < Points[1].Item1 || 
        //     inVal > Points[0].Item1 && Points[0].Item1 > Points[1].Item1)
        //     return Points[0].Item2;
        //
        // var left  = 0;
        // var right = 1;
        //
        // // Can't assume left is less than right
        // var between = Points[left].Item1 <= inVal && inVal <= Points[right].Item1
        //     || Points[right].Item1 <= inVal && inVal <= Points[left].Item1;
        //
        // while (left < Points.Count && !between) {
        //     left++;
        //     right++;
        //     
        //     between = Points[left].Item1 <= inVal && inVal <= Points[right].Item1
        //         || Points[right].Item1 <= inVal && inVal <= Points[left].Item1;
        // }
        //
        // // Beyond right limit
        // if (right > Points.Count)
        //     return Points[left].Item2;
        //
        // var pct = (inVal - Points[left].Item1) / (Points[right].Item1 - Points[left].Item1);
        // return JMath.Lerp(Points[left].Item2, Points[right].Item2, pct);
    }
}