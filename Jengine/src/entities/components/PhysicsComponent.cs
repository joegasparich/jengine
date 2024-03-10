using System.Numerics;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;

namespace JEngine.entities;

// TODO: PhysicsComponentData

public class PhysicsComponent : Component {
    private Body body;

    public PhysicsComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);

        var def = new BodyDef();
        def.BodyType = BodyType.DynamicBody;
        def.Position = entity.Transform.LocalPosition;
        def.LinearDamping = 0.05f;
        def.FixedRotation = true;

        var shape = new CircleShape {
            Radius = 0.5f
        };
        var fixtureDef = new FixtureDef {
            Shape = shape,
            Density = 10f,
            Friction = 0f,
        };

        body = Find.Game.physics.RegisterBody(def, entity);
        body.CreateFixture(fixtureDef);
    }

    public override void PreUpdate() {
        if (entity.Transform.LocalPosition != body.GetPosition())
            body.SetTransform(entity.Transform.LocalPosition, 0f);

        // TODO: Rotation as well
    }

    public override void PostUpdate() {
        entity.Transform.LocalPosition = body.GetPosition();
    }

    public void AddForce(Vector2 force) {
        body.ApplyForce(force, body.GetLocalCenter(), true);
    }
}