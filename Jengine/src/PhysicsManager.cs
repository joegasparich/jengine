using System.Numerics;
using Box2DSharp.Collision.Collider;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Contacts;
using JEngine.entities;
using JEngine.util;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace JEngine;

public interface IContactable {
    public Body Body { get; }
    public void OnContact(Body other);
}

public class PhysicsManager : IContactListener {
    public  World                    world;
    private HashSet<IContactable>    contactListeners = new();
    private Dictionary<Body, Entity> bodyToEntity     = new();
    private List<Body>               bodiesToDestroy  = new();

    private float msPerUpdate;
    private bool debug;

    public void Init() {
        msPerUpdate = (float)Find.Game.gameConfig.msPerUpdate;

        world = new World(Vector2.Zero);
        world.SetContactListener(this);
    }

    public void Update() {
        world.Step(msPerUpdate, 6, 3);
        
        foreach (var body in bodiesToDestroy) {
            world.DestroyBody(body);
        }
        bodiesToDestroy.Clear();
    }

    public void DrawLate() {
        if (debug)
            DrawDebug();
    }

    public void OnInput(InputEvent evt) {
        if (evt.consumed)
            return;

        if (evt.keyDown == KeyboardKey.F3) {
            debug = !debug;
            evt.Consume();
        }
    }

    public Body RegisterBody(BodyDef def, Entity entity) {
        var body = world.CreateBody(def);
        bodyToEntity[body] = entity;

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
        foreach (var body in world.BodyList) {
            foreach (var fixture in body.FixtureList) {
                if (fixture.Shape is CircleShape circle) {
                    Drawing.DrawCircleLines(body.GetPosition() * Find.Game.gameConfig.worldScalePx, circle.Radius * Find.Game.gameConfig.worldScalePx, Color.Red, (int)Depth.Debug);
                } else if (fixture.Shape is PolygonShape poly) {
                    var vertices = poly.Vertices;
                    for (var i = 0; i < poly.Count; i++) {
                        var vertex     = vertices[i];
                        var nextVertex = vertices[(i + 1) % poly.Count];
                        Drawing.DrawLine(
                            (body.GetPosition() + vertex) * Find.Game.gameConfig.worldScalePx,
                            (body.GetPosition() + nextVertex) * Find.Game.gameConfig.worldScalePx,
                            Color.Red,
                            (int)Depth.Debug
                        );
                    }
                }
            }
        };
    }

    public Body RegisterCollider(Rectangle rect) {
        var def   = new BodyDef();
        var shape = new PolygonShape();
        shape.Set(rect.Vertices());
        var body = world.CreateBody(def);
        body.CreateFixture(new FixtureDef { Shape = shape, Friction = 0 });
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