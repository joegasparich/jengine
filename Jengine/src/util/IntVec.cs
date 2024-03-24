using System.Numerics;

namespace JEngine.util;

public struct IntVec2 : IEquatable<IntVec2> {
    public int X;
    public int Y;

    public static IntVec2 Zero => new(0, 0);
    public static IntVec2 One  => new(1, 1);

    public IntVec2() {
        X = 0;
        Y = 0;
    }
    public IntVec2(int n) {
        X = n;
        Y = n;
    }
    public IntVec2(int x, int y) {
        X = x;
        Y = y;
    }

    public static IntVec2 operator +(IntVec2 a, IntVec2 b) {
        return new IntVec2(a.X + b.X, a.Y + b.Y);
    }
    public static IntVec2 operator -(IntVec2 a, IntVec2 b) {
        return new IntVec2(a.X - b.X, a.Y - b.Y);
    }
    public static IntVec2 operator *(IntVec2 a, IntVec2 b) {
        return new IntVec2(a.X * b.X, a.Y * b.Y);
    }
    public static IntVec2 operator *(IntVec2 a, int b) {
        return new IntVec2(a.X * b, a.Y * b);
    }
    public static IntVec2 operator /(IntVec2 a, int b) {
        return new IntVec2(a.X / b, a.Y / b);
    }
    public static Vector2 operator /(IntVec2 a, float b) {
        return new Vector2(a.X / b, a.Y / b);
    }
    public static bool operator ==(IntVec2 a, IntVec2 b) {
        return a.Equals(b);
    }
    public static bool operator !=(IntVec2 a, IntVec2 b) {
        return !a.Equals(b);
    }

    public static implicit operator Vector2(IntVec2 v) => new(v.X, v.Y);
    public Vector2 ToVector2() => new(X, Y);

    public bool Equals(IntVec2 other) {
        return X == other.X && Y == other.Y;
    }
    
    public void Deconstruct(out int x, out int y) {
        x = X;
        y = Y;
    }

    public string ToString() {
        return $"{X}, {Y}";
    }
    public static IntVec2 FromString(string s) {
        if (s.NullOrEmpty()) 
            return Zero;
        
        var split = s.Split(',');
        if (split.Length != 2) 
            return Zero;
        
        if (!int.TryParse(split[0], out var x)) 
            return Zero;
            
        if (!int.TryParse(split[1], out var y)) 
            return Zero;
            
        return new IntVec2(x, y);
    }
}

public struct IntVec3 : IEquatable<IntVec3> {
    public int X;
    public int Y;
    public int Z;

    public static IntVec2 Zero => new(0, 0);
    public static IntVec2 One  => new(1, 1);

    public IntVec3() {
        X = 0;
        Y = 0;
        Z = 0;
    }
    public IntVec3(int n) {
        X = n;
        Y = n;
        Z = n;
    }
    public IntVec3(int x, int y) {
        this.X = x;
        this.Y = y;
        Z      = 0;
    }
    public IntVec3(int x, int y, int z) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public static IntVec3 operator +(IntVec3 a, IntVec3 b) {
        return new IntVec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    public static IntVec3 operator -(IntVec3 a, IntVec3 b) {
        return new IntVec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    public static IntVec3 operator *(IntVec3 a, IntVec3 b) {
        return new IntVec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }
    public static IntVec3 operator *(IntVec3 a, int b) {
        return new IntVec3(a.X * b, a.Y * b, a.Z * b);
    }
    public static IntVec3 operator /(IntVec3 a, int b) {
        return new IntVec3(a.X / b, a.Y / b, a.Z / b);
    }
    public static bool operator ==(IntVec3 a, IntVec3 b) {
        return a.Equals(b);
    }
    public static bool operator !=(IntVec3 a, IntVec3 b) {
        return !a.Equals(b);
    }

    public static implicit operator Vector3(IntVec3 v) => new(v.X, v.Y, v.Z);
    
    public void Deconstruct(out float x, out float y, out float z) {
        x = this.X;
        y = this.Y;
        z = this.Z;
    }

    public bool Equals(IntVec3 other) {
        return X == other.X && Y == other.Y && Z == other.Z;
    }
}