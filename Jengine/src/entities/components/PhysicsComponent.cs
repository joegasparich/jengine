using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;

namespace JEngine.entities;

public class PhysicsComponent : Component {
    private Body body;

    public PhysicsComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);

        var def = new BodyDef();
        def.type = BodyType.Dynamic;
        def.position = entity.pos;
        def.linearDamping = 0.05f;

        var shape = new CircleShape {
            Radius = 0.5f
        };
        var fixtureDef = new FixtureDef {
            shape = shape,
            density = 10f,
            friction = 0f,

        };

        body = Find.Game.physics.world.CreateBody(def);
        body.CreateFixture(fixtureDef);
    }

    public override void PreUpdate() {
        if (entity.pos != body.Position)
            body.SetTransform(entity.pos, 0f);
    }

    public override void PostUpdate() {
        entity.pos = body.Position;
    }

    public void AddForce(Vector2 force) {
        body.ApplyForce(force, body.GetLocalCenter());
    }
}