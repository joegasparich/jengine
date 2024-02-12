using System.Numerics;
using Raylib_cs;

namespace JEngine.util; 

public static class RectangleExtension {
    public static Vector2 Position(this Rectangle rect) {
        return new Vector2(rect.X, rect.Y);
    }
    public static Vector2 Dimensions(this Rectangle rect) {
        return new Vector2(rect.Width, rect.Height);
    }
    public static Vector2 Center(this Rectangle rect) {
        return new Vector2(rect.X + rect.Width/2, rect.Y + rect.Height/2);
    }
    public static Vector2[] Vertices(this Rectangle rect) {
        return [
            new Vector2(rect.X, rect.Y),
            new Vector2(rect.X + rect.Width, rect.Y),
            new Vector2(rect.X + rect.Width, rect.Y + rect.Height),
            new Vector2(rect.X, rect.Y + rect.Height)
        ];
    }
    public static bool Contains(this Rectangle rect, Vector2 point) {
        return JMath.PointInRect(rect, point);
    }
    public static float XMax(this Rectangle rect) {
        return rect.X + rect.Width;
    }
    public static float YMax(this Rectangle rect) {
        return rect.Y + rect.Height;
    }
    public static Rectangle ContractedBy(this Rectangle rect, float amt) {
        return new Rectangle(rect.X + amt, rect.Y + amt, rect.Width - amt * 2, rect.Height - amt * 2);
    }
    public static Rectangle ContractedByExt(this Rectangle rect, float xMin = 0, float yMin = 0, float xMax = 0, float yMax = 0) {
        return new Rectangle(rect.X + xMin, rect.Y + yMin, rect.Width - xMin - xMax, rect.Height - yMin - yMax);
    }
    public static Rectangle ExpandedBy(this Rectangle rect, float amt) {
        return new Rectangle(rect.X - amt, rect.Y - amt, rect.Width + amt * 2, rect.Height + amt * 2);
    }
    public static Rectangle ExpandedByExt(this Rectangle rect, float xMin = 0, float yMin = 0, float xMax = 0, float yMax = 0) {
        return new Rectangle(rect.X - xMin, rect.Y - yMin, rect.Width + xMin + xMax, rect.Height + yMin + yMax);
    }
    public static Rectangle TopPx(this Rectangle rect, float px) {
        return new Rectangle(rect.X, rect.Y, rect.Width, px);
    }
    public static Rectangle BottomPx(this Rectangle rect, float px) {
        return new Rectangle(rect.X, rect.Y + rect.Height - px, rect.Width, px);
    }
    public static Rectangle LeftPx(this Rectangle rect, float px) {
        return new Rectangle(rect.X, rect.Y, px, rect.Height);
    }
    public static Rectangle RightPx(this Rectangle rect, float px) {
        return new Rectangle(rect.X + rect.Width - px, rect.Y, px, rect.Height);
    }
    public static Rectangle TopPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height * pct);
    }
    public static Rectangle BottomPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.X, rect.Y + rect.Height * (1 - pct), rect.Width, rect.Height * pct);
    }
    public static Rectangle LeftPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.X, rect.Y, rect.Width * pct, rect.Height);
    }
    public static Rectangle RightPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.X + rect.Width * (1 - pct), rect.Y, rect.Width * pct, rect.Height);
    }
    public static Rectangle TopHalf(this Rectangle rect) {
        return rect.TopPct(0.5f);
    }
    public static Rectangle BottomHalf(this Rectangle rect) {
        return rect.BottomPct(0.5f);
    }
    public static Rectangle LeftHalf(this Rectangle rect) {
        return rect.LeftPct(0.5f);
    }
    public static Rectangle RightHalf(this Rectangle rect) {
        return rect.RightPct(0.5f);
    }
    public static Rectangle Multiply(this Rectangle rect, float amt) {
        return new Rectangle(rect.X * amt, rect.Y * amt, rect.Width * amt, rect.Height * amt);
    }
    public static Rectangle OffsetBy(this Rectangle rect, float x, float y) {
        return new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height);
    }
}