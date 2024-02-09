using System.Numerics;

namespace JEngine.util; 

public static class IntVecExtension {
    public static float Distance(this IntVec2 tile, IntVec2 other) {
        return MathF.Sqrt(MathF.Pow(tile.x - other.x, 2) + MathF.Pow(tile.y - other.y, 2));
    }
    public static float DistanceSquared(this IntVec2 tile, IntVec2 other) {
        return MathF.Pow(tile.x - other.x, 2) + MathF.Pow(tile.y - other.y, 2);
    }
    public static bool InDistOf(this IntVec2 a, IntVec2 b, float range) {
        return a.DistanceSquared(b) < range * range;
    }
    public static Vector2 Centre(this IntVec2 tile) {
        return new Vector2(tile.x + 0.5f, tile.y + 0.5f);
    }
    public static IntVec2 Closest(this IEnumerable<IntVec2> tiles, Vector2 pos) {
        var result = new Vector2();
        var first  = true;
        foreach (var tile in tiles) {
            if (first) {
                first  = false;
                result = tile;
                continue;
            }

            result = result.DistanceSquared(pos) < tile.ToVector2().DistanceSquared(pos) ? result : tile;
        }

        return result.Floor();
    }
}