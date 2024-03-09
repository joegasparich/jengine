using System.Numerics;
using JEngine.entities;

namespace JEngine.util;

public class Transform : ISerialisable {
    public Vector2    LocalPosition;
    public float      LocalRotation;
    public Vector2    LocalScale;

    // Currently tied to entities, will need some kind of transform store if we want to be able to
    // serialised references to parents without a reference to the entity
    public Entity Entity;

    public Matrix3x2 LocalMatrix => Matrix3x2.CreateScale(LocalScale) *
                                    Matrix3x2.CreateRotation(LocalRotation * JMath.DegToRad) *
                                    Matrix3x2.CreateTranslation(LocalPosition);

    public Matrix3x2 GlobalMatrix {
        get {
            if (Entity.parent == null)
                return LocalMatrix;

            return
                Matrix3x2.CreateScale(LocalScale) *
                Matrix3x2.CreateScale(Entity.parent.Transform.LocalScale) *
                Matrix3x2.CreateRotation(LocalRotation * JMath.DegToRad) *
                Matrix3x2.CreateTranslation(LocalPosition) *
                Matrix3x2.CreateRotation(Entity.parent.Transform.LocalRotation * JMath.DegToRad) *
                Matrix3x2.CreateTranslation(Entity.parent.Transform.LocalPosition);
        }
    }

    public Vector2 GlobalPosition => Entity.parent == null ? LocalPosition : GlobalMatrix.Translation;
    public float   GlobalRotation => Entity.parent == null ? LocalRotation : (float)Math.Atan2(GlobalMatrix.M21, GlobalMatrix.M11) * JMath.RadToDeg;
    public Vector2 GlobalScale    => Entity.parent == null ? LocalScale : new(MathF.Sqrt(GlobalMatrix.M11 * GlobalMatrix.M11 + GlobalMatrix.M12 * GlobalMatrix.M12),
                                         MathF.Sqrt(GlobalMatrix.M21 * GlobalMatrix.M21 + GlobalMatrix.M22 * GlobalMatrix.M22));

    public Transform(Entity entity) {
        Entity = entity;

        LocalPosition = Vector2.Zero;
        LocalRotation = 0;
        LocalScale    = Vector2.One;
    }
    public Transform(Entity entity, Vector2 position, float rotation, Vector2 scale) {
        Entity = entity;

        LocalPosition = position;
        LocalRotation = rotation;
        LocalScale    = scale;
    }

    public void Serialise() {
        Find.SaveManager.ArchiveValue("localPosition", ref LocalPosition);
        Find.SaveManager.ArchiveValue("localRotation", ref LocalRotation);
        Find.SaveManager.ArchiveValue("localScale", ref LocalScale);

        Find.SaveManager.ArchiveRef("entity", ref Entity);
    }
}