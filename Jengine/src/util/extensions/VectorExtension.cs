﻿using System.Numerics;

namespace JEngine.util; 

public static class VectorExtension {
    public static IntVec2 Floor(this Vector2 vec) {
        return new IntVec2(vec.X.FloorToInt(), vec.Y.FloorToInt());
    }
    public static IntVec2 Round(this Vector2 vec) {
        return new IntVec2(vec.X.RoundToInt(), vec.Y.RoundToInt());
    }
    public static void Deconstruct(this Vector2 v, out float x, out float y) {
        x = v.X;
        y = v.Y;
    }
    public static float Distance(this Vector2 a, Vector2 b) {
        return MathF.Sqrt(MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2));
    }
    public static float DistanceSquared(this Vector2 a, Vector2 b) {
        return MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2);
    }
    public static bool InRangeOf(this Vector2 a, Vector2 b, float range) {
        return a.DistanceSquared(b) < range * range;
    }
    public static float Magnitude(this Vector2 v) {
        return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
    }
    public static Vector2 Normalised(this Vector2 v) {
        if (v.LengthSquared() == 0) 
            return Vector2.Zero;
        
        return v / v.Length();
    }
    public static Vector2 Truncate(this Vector2 v, float max) {
        if (v.LengthSquared() > max * max) 
            return v.Normalised() * max;
        
        return v;
    }
    public static float AngleDeg(this Vector2 v) {
        return MathF.Atan2(v.Y, v.X) * JMath.RadToDeg;
    }
    public static Vector3 ToVector3(this Vector2 v) {
        return new Vector3(v.X, v.Y, 0);
    }
    
    // Vector3 //
    public static void Deconstruct(this Vector3 v, out float x, out float y, out float z) {
        x = v.X;
        y = v.Y;
        z = v.Z;
    }
    
    public static Vector2 Rotate(this Vector2 v, float angleDeg) {
        var s = MathF.Sin(angleDeg * JMath.DegToRad);
        var c = MathF.Cos(angleDeg * JMath.DegToRad);
        return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
    }

    public static Vector2 RotateAround(this Vector2 a, Vector2 b, float angleDeg) {
        var s    = MathF.Sin(angleDeg * JMath.DegToRad);
        var c    = MathF.Cos(angleDeg * JMath.DegToRad);
        var x    = a.X - b.X;
        var y    = a.Y - b.Y;
        var xnew = x * c - y * s;
        var ynew = x * s + y * c;
        return new Vector2(xnew + b.X, ynew + b.Y);
    }
}