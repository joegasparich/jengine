using System.Numerics;
using JEngine;
using JEngine.entities;

namespace SlasherExample.entities;

public class SimplePhysicsComponent : Component {
    // Config
    public float Mass     = 50;
    public float Friction = 0.5f;

    // State
    private Vector2 velocity = Vector2.Zero;
    private Vector2 force    = Vector2.Zero;

    public SimplePhysicsComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Update() {
        entity.Transform.LocalPosition += velocity;
    }

    public override void PostUpdate() {
        Debug.Assert(Mass     > 0);
        Debug.Assert(Friction > 0);

        // Add force
        velocity += force / Mass;
        // Apply dampening
        velocity *= 1 / (1 + Friction);

        force = Vector2.Zero;
    }

    public void AddForce(Vector2 force) {
        this.force += force;
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("velocity", ref velocity);
        Find.SaveManager.ArchiveValue("force", ref force);
        Find.SaveManager.ArchiveValue("mass", ref Mass);
        Find.SaveManager.ArchiveValue("friction", ref Friction);
    }
}