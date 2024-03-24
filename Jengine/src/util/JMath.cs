using System.Numerics;
using Raylib_cs;

namespace JEngine.util;

public static class JMath {
    // Modify values
    public static float Lerp(float from, float to,  float pct) =>  from + (to - from) * pct;
    public static float Normalise(float val,  float min, float max) =>  (val - min) / (max - min);
    public static float Clamp(float val,  float min, float max) =>  Math.Max(min, Math.Min(max, val));
    public static float Clamp01(float val) =>  Math.Max(0, Math.Min(1, val));
    public static float Min(params float[] vals) => vals.Min();
    public static float Max(params float[] vals) => vals.Max();
    public static int PositiveMod(int x, int m) => (x % m + m) % m;
    
    // Trig
    public const float DegToRad = MathF.PI / 180.0f;
    public const float RadToDeg = 180f / (float)Math.PI;
    
    // Vectors
    public static Vector2 Xy(this Vector3 v) => new Vector2(v.X, v.Y);
    public static Vector2 Xz(this Vector3 v) => new Vector2(v.X, v.Z);
    public static Vector2 Yz(this Vector3 v) => new Vector2(v.Y, v.Z);
    
    // Collision
    public static bool PointInRect(Rectangle rect, Vector2 point) {
        return point.X >= rect.X && point.X <= rect.X + rect.Width &&
               point.Y > rect.Y && point.Y < rect.Y + rect.Height;        
    }
    
    public static bool PointInCircle(Vector2 circlePos, float radius, Vector2 point) {
        var dx = circlePos.X - point.X;
        var dy = circlePos.Y - point.Y;
        return dx * dx + dy * dy < radius * radius;
    }
    
    public static bool CircleIntersectsRect(Vector2 boxPos, Vector2 boxDim, Vector2 circlePos, float circleRad) {
        var distX = MathF.Abs(circlePos.X - boxPos.X - boxDim.X / 2);
        var distY = MathF.Abs(circlePos.Y - boxPos.Y - boxDim.Y / 2);

        if (distX > boxDim.X / 2 + circleRad)
            return false;
        if (distY > boxDim.Y / 2 + circleRad)
            return false;

        if (distX <= boxDim.X / 2)
            return true;
        if (distY <= boxDim.Y / 2)
            return true;

        var dx = distX - boxDim.X / 2;
        var dy = distY - boxDim.Y / 2;
        return dx * dx + dy * dy <= circleRad * circleRad;
    }
    
    public static bool PointInPolygon(List<Vector2> polygon, Vector2 point)
    {
        var result = false;
        var j      = polygon.Count - 1;
        for (var i = 0; i < polygon.Count; i++) {
            if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y || polygon[j].Y < point.Y && polygon[i].Y >= point.Y) {
                if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    result = !result;
            }
            j = i;
        }
        return result;
    }
}