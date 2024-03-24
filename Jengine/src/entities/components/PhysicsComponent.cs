using System.Numerics;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;

namespace JEngine.entities;

public enum ShapeType {
    Circle,
    Rectangle,
}

public struct Collider {
    public ShapeType Shape;
    public float     Radius;
}

public class PhysicsComponentData : ComponentData {
    public Collider Collider;
}


public class PhysicsComponent(Entity entity, ComponentData? data = null) : Component(entity, data) {
    public static Type DataType => typeof(PhysicsComponentData);

    private Body _body;
    
    public PhysicsComponentData? Data => _data as PhysicsComponentData;

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);

        if (Data == null)
            return;

        var def = new BodyDef {
            BodyType      = BodyType.DynamicBody,
            Position      = Entity.Transform.LocalPosition,
            LinearDamping = 0.05f,
            FixedRotation = true
        };

        _body = Find.Game.Physics.RegisterBody(def, Entity);

        Shape shape = Data.Collider.Shape switch {
            ShapeType.Circle => new CircleShape { Radius = Data.Collider.Radius },
            _                => throw new NotImplementedException()
        };
        
        var fixtureDef = new FixtureDef {
            Shape = shape,
            Density = 10f,
            Friction = 0f,
        };

        _body.CreateFixture(fixtureDef);
    }

    public override void PreUpdate() {
        if (Entity.Transform.LocalPosition != _body.GetPosition())
            _body.SetTransform(Entity.Transform.LocalPosition, 0f);

        // TODO: Rotation as well
    }

    public override void PostUpdate() {
        Entity.Transform.LocalPosition = _body.GetPosition();
    }

    public void AddForce(Vector2 force) {
        _body.ApplyForce(force, _body.GetLocalCenter(), true);
    }
}