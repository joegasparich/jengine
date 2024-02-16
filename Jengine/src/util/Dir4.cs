﻿namespace Jengine.util;

public class Dir4 : IEquatable<Dir4>
{
    public static readonly Dir4 North = new Dir4(0);
    public static readonly Dir4 East  = new Dir4(1);
    public static readonly Dir4 South = new Dir4(2);
    public static readonly Dir4 West  = new Dir4(3);

    private byte dirInt;

    public Dir4(byte dir) {
        dirInt = dir;
    }

    public override bool Equals(object obj) {
        if (!(obj is Dir4))
            return false;

        return Equals((Dir4)obj);
    }

    public bool Equals(Dir4 other) {
        return dirInt == other.dirInt;
    }

    public static bool operator ==(Dir4 a, Dir4 b) {
        return a.Equals(b);
    }

    public static bool operator !=(Dir4 a, Dir4 b) {
        return !a.Equals(b);
    }
}