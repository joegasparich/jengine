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


public class PhysicsComponent : Component {
    public static Type DataType => typeof(PhysicsComponentData);

    private Body body;
    
    public PhysicsComponentData Data => (PhysicsComponentData)data;

    public PhysicsComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);

        var def = new BodyDef();
        def.BodyType = BodyType.DynamicBody;
        def.Position = entity.Transform.LocalPosition;
        def.LinearDamping = 0.05f;
        def.FixedRotation = true;
        
        body = Find.Game.physics.RegisterBody(def, entity);

        Shape shape;
        
        switch (Data.Collider.Shape) {
            case ShapeType.Circle:
                shape = new CircleShape {
                    Radius = Data.Collider.Radius
                };
                break;
            default:
                throw new NotImplementedException();
        }
        var fixtureDef = new FixtureDef {
            Shape = shape,
            Density = 10f,
            Friction = 0f,
        };

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