namespace JEngine.util;

public class Dir4 : IEquatable<Dir4>
{
    public static readonly Dir4 North = new(0);
    public static readonly Dir4 East  = new(1);
    public static readonly Dir4 South = new(2);
    public static readonly Dir4 West  = new(3);

    private byte _dirInt;

    public Dir4(byte dir) {
        _dirInt = dir;
    }

    public override bool Equals(object obj) {
        if (!(obj is Dir4))
            return false;

        return Equals((Dir4)obj);
    }

    public bool Equals(Dir4 other) {
        return _dirInt == other._dirInt;
    }

    public static bool operator ==(Dir4 a, Dir4 b) {
        return a.Equals(b);
    }

    public static bool operator !=(Dir4 a, Dir4 b) {
        return !a.Equals(b);
    }

    public IntVec2 ToIntVec() {
        if (this == North)
            return new IntVec2(0, -1);
        if (this == East)
            return new IntVec2(1, 0);
        if (this == South)
            return new IntVec2(0, 1);
        if (this == West)
            return new IntVec2(-1, 0);
        
        return IntVec2.Zero;
    }
}