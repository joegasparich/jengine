using Box2DSharp.Collision.Collider;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Contacts;
using Box2DSharp.Dynamics.Joints;
using JEngine.entities;
using JEngine.util;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Vector2 = System.Numerics.Vector2;

namespace JEngine;

public interface IContactable {
    public Body Body { get; }
    public void OnContact(Body other);
}

public class PhysicsManager : IContactListener {
    public  World                                World;
    private HashSet<IContactable>                contactListeners = new();
    private Dictionary<Body, Entity>             bodyToEntity     = new();
    private List<Body>                           bodiesToDestroy  = new();
    private Dictionary<string, (ushort, ushort)> collisionLayers  = new();

    private Body ground;

    private float msPerUpdate;
    private bool debug;

    public void Init() {
        msPerUpdate = (float)Find.Game.GameConfig.MsPerUpdate;

        World = new World(Vector2.Zero);
        World.SetContactListener(this);

        ground = World.CreateBody(new BodyDef {
            BodyType = BodyType.StaticBody,
            Position = new Vector2(0, 0)
        });
        var groundShape = new PolygonShape();  
        groundShape.SetAsBox(1000, 1000);
        ground.CreateFixture(new FixtureDef {
            Density     = 0.0f,
            Shape       = groundShape,
            Restitution = 0f,
            Friction    = 0f
        });
    }

    public void Update() {
        World.Step(msPerUpdate / 1000f, 6, 3);
        
        foreach (var body in bodiesToDestroy) {
            World.DestroyBody(body);
        }
        bodiesToDestroy.Clear();
    }

    public void DrawLate() {
        if (debug)
            DrawDebug();
    }

    public void OnInput(InputEvent evt) {
        if (evt.Consumed)
            return;

        if (evt.KeyDown == KeyboardKey.F3) {
            debug = !debug;
            evt.Consume();
        }
    }
    
    public void RegisterCollisionLayer(string name, ushort layer, ushort mask) {
        collisionLayers[name] = (layer, mask);
    }
    public ushort GetCollisionLayer(string name) => !collisionLayers.TryGetValue(name, out var value) ? (ushort)0 : value.Item1;
    public ushort GetCollisionMask(string  name) => !collisionLayers.TryGetValue(name, out var value) ? (ushort)0 : value.Item2;

    public Body RegisterBody(BodyDef def, Entity entity) {
        var body = World.CreateBody(def);
        bodyToEntity[body] = entity;

        // if (body.BodyType == BodyType.DynamicBody) {
        //     var jointDef = new FrictionJointDef();
        //     jointDef.Initialize(ground, body, Vector2.Zero);
        //     jointDef.MaxForce = 9f;
        //     World.CreateJoint(jointDef);
        // }

        return body;
    }
    
    public void DestroyBody(Body body) {
        bodiesToDestroy.Add(body);
        bodyToEntity.Remove(body);
    }

    public Entity? GetEntityFromBody(Body body) {
        if (!bodyToEntity.ContainsKey(body))
            return null;

        return bodyToEntity[body];
    }

    private void DrawDebug() {
        foreach (var body in World.BodyList) {
            foreach (var fixture in body.FixtureList) {
                if (fixture.Shape is CircleShape circle) {
                    Drawing.DrawCircleLines(body.GetPosition() * Find.Game.GameConfig.WorldScalePx, circle.Radius * Find.Game.GameConfig.WorldScalePx, Color.Red, (int)Depth.Debug);
                } else if (fixture.Shape is PolygonShape poly) {
                    var vertices = poly.Vertices;
                    for (var i = 0; i < poly.Count; i++) {
                        var vertex     = vertices[i];
                        var nextVertex = vertices[(i + 1) % poly.Count];
                        Drawing.DrawLine(
                            (body.GetPosition() + vertex) * Find.Game.GameConfig.WorldScalePx,
                            (body.GetPosition() + nextVertex) * Find.Game.GameConfig.WorldScalePx,
                            Color.Red,
                            (int)Depth.Debug
                        );
                    }
                }
            }
        };
    }

    public Body RegisterCollider(Rectangle rect, string collisionLayer) {
        var def   = new BodyDef();
        var shape = new PolygonShape();
        shape.Set(rect.Vertices());
        var body       = World.CreateBody(def);
        var fixtureDef = new FixtureDef { Shape = shape, Friction = 0 };
        fixtureDef.Filter.CategoryBits = collisionLayers[collisionLayer].Item1;
        fixtureDef.Filter.MaskBits     = collisionLayers[collisionLayer].Item2;
        fixtureDef.Friction            = 0.5f;
        fixtureDef.Restitution         = 0.3f;
        body.CreateFixture(fixtureDef);
        return body;
    }

    public void RegisterContactListener(IContactable listener) {
        contactListeners.Add(listener);
    }
    public void UnregisterContactListener(IContactable listener) {
        contactListeners.Remove(listener);
    }

    public void BeginContact(Contact contact) {
        foreach (var listener in contactListeners) {
            if (contact.FixtureA.Body == listener.Body)
                listener.OnContact(contact.FixtureB.Body);
            if (contact.FixtureB.Body == listener.Body)
                listener.OnContact(contact.FixtureA.Body);
        }
    }
    public void EndContact(Contact contact) {}
    public void PreSolve(Contact contact, in Manifold oldManifold) {}
    public void PostSolve(Contact contact, in ContactImpulse impulse) {}
}