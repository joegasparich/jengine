using System.Numerics;

namespace JEngine.util;

public struct Transform {
    public float m11, m12, m13;
    public float m21, m22, m23;
    public float m31, m32, m33;
    
    public Vector2 Translation {
        get => new(m13, m23);
    }
    public float Rotation {
        get => MathF.Atan2(m21, m11);
    }
    public Vector2 Scale {
        get => new(m11, m22);
    }
    
    public Transform(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33) {
        this.m11 = m11;
        this.m12 = m12;
        this.m13 = m13;
        this.m21 = m21;
        this.m22 = m22;
        this.m23 = m23;
        this.m31 = m31;
        this.m32 = m32;
        this.m33 = m33;
    }
    
    public static Transform Identity() {
        return new Transform(1, 0, 0, 0, 1, 0, 0, 0, 1);
    }
    
    public static Transform TRS(Vector2 position, float angle, Vector2 scale) {
        return CreateTranslation(position) * CreateRotation(angle) * CreateScale(scale);
    }
    
    public static Transform CreateTranslation(float x, float y) {
        return new Transform(1, 0, x, 0, 1, y, 0, 0, 1);
    }
    
    public static Transform CreateTranslation(Vector2 position) {
        return CreateTranslation(position.X, position.Y);
    }
    
    public static Transform CreateRotation(float angle) {
        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);
        return new Transform(cos, -sin, 0, sin, cos, 0, 0, 0, 1);
    }
    
    public static Transform CreateRotation(float angle, Vector2 center) {
        return CreateTranslation(center) * CreateRotation(angle) * CreateTranslation(-center);
    }
    
    public static Transform CreateScale(float x, float y) {
        return new Transform(x, 0, 0, 0, y, 0, 0, 0, 1);
    }
    
    public static Transform CreateScale(Vector2 scale) {
        return CreateScale(scale.X, scale.Y);
    }
    
    public static Transform operator *(Transform a, Transform b) {
        return new Transform(
            a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31,
            a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32,
            a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33,
            a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31,
            a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32,
            a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33,
            a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31,
            a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32,
            a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33
        );
    }
    
    public static Vector2 operator *(Transform a, Vector2 b) {
        return new Vector2(
            a.m11 * b.X + a.m12 * b.Y + a.m13,
            a.m21 * b.X + a.m22 * b.Y + a.m23
        );
    }
    
    public static Transform operator *(Transform a, float b) {
        return new Transform(
            a.m11 * b, a.m12 * b, a.m13 * b,
            a.m21 * b, a.m22 * b, a.m23 * b,
            a.m31 * b, a.m32 * b, a.m33 * b
        );
    }
    
    public static Transform operator +(Transform a, Transform b) {
        return new Transform(
            a.m11 + b.m11, a.m12 + b.m12, a.m13 + b.m13,
            a.m21 + b.m21, a.m22 + b.m22, a.m23 + b.m23,
            a.m31 + b.m31, a.m32 + b.m32, a.m33 + b.m33
        );
    }
    
    public static Transform operator -(Transform a, Transform b) {
        return new Transform(
            a.m11 - b.m11, a.m12 - b.m12, a.m13 - b.m13,
            a.m21 - b.m21, a.m22 - b.m22, a.m23 - b.m23,
            a.m31 - b.m31, a.m32 - b.m32, a.m33 - b.m33
        );
    }
    
    public static bool operator ==(Transform a, Transform b) {
        return a.m11 == b.m11 && a.m12 == b.m12 && a.m13 == b.m13 && a.m21 == b.m21 && a.m22 == b.m22 && a.m23 == b.m23 && a.m31 == b.m31 && a.m32 == b.m32 && a.m33 == b.m33;
    }
    
    public static bool operator !=(Transform a, Transform b) {
        return a.m11 != b.m11 || a.m12 != b.m12 || a.m13 != b.m13 || a.m21 != b.m21 || a.m22 != b.m22 || a.m23 != b.m23 || a.m31 != b.m31 || a.m32 != b.m32 || a.m33 != b.m33;
    }
    
    public override bool Equals(object obj) {
        return obj is Transform matrix && this == matrix;
    }
    
    public override int GetHashCode() {
        return m11.GetHashCode() ^ m12.GetHashCode() ^ m13.GetHashCode() ^ m21.GetHashCode() ^ m22.GetHashCode() ^ m23.GetHashCode() ^ m31.GetHashCode() ^ m32.GetHashCode() ^ m33.GetHashCode();
    }
    
    public override string ToString() {
        return $"{{m11:{m11} m12:{m12} m13:{m13} m21:{m21} m22:{m22} m23:{m23} m31:{m31} m32:{m32} m33:{m33}}}";
    }
    
    public static Transform Transpose(Transform matrix) {
        return new Transform(
            matrix.m11, matrix.m21, matrix.m31,
            matrix.m12, matrix.m22, matrix.m32,
            matrix.m13, matrix.m23, matrix.m33
        );
    }
    
    public static Transform Invert(Transform matrix) {
        var det = matrix.m11 * (matrix.m22 * matrix.m33 - matrix.m23 * matrix.m32) - matrix.m12 * (matrix.m21 * matrix.m33 - matrix.m23 * matrix.m31) + matrix.m13 * (matrix.m21 * matrix.m32 - matrix.m22 * matrix.m31);
        if (det == 0) {
            return matrix;
        }
        var invDet = 1 / det;
        return new Transform(
            (matrix.m22 * matrix.m33 - matrix.m23 * matrix.m32) * invDet,
            (matrix.m13 * matrix.m32 - matrix.m12 * matrix.m33) * invDet,
            (matrix.m12 * matrix.m23 - matrix.m13 * matrix.m22) * invDet,
            (matrix.m23 * matrix.m31 - matrix.m21 * matrix.m33) * invDet,
            (matrix.m11 * matrix.m33 - matrix.m13 * matrix.m31) * invDet,
            (matrix.m21 * matrix.m13 - matrix.m11 * matrix.m23) * invDet,
            (matrix.m21 * matrix.m32 - matrix.m22 * matrix.m31) * invDet,
            (matrix.m12 * matrix.m31 - matrix.m11 * matrix.m32) * invDet,
            (matrix.m11 * matrix.m22 - matrix.m12 * matrix.m21) * invDet
        );
    }
}