using System.Numerics;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using JEngine.entities;
using JEngine.util;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace JEngine;

public class PhysicsManager {
    public World world;
    private Dictionary<Body, Entity> bodyToEntity = new();

    private float msPerUpdate;

    public void Init() {
        msPerUpdate = (float)Find.Game.gameConfig.msPerUpdate;

        world = new World(Vector2.Zero);
    }

    public void Update() {
        world.Step(msPerUpdate, 6, 3);
    }

    public void DrawLate() {
        // DrawDebug();
    }

    public Body RegisterBody(BodyDef def, Entity entity) {
        var body = world.CreateBody(def);
        bodyToEntity[body] = entity;

        return body;
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

    public void RegisterCollider(Rectangle rect) {
        var def   = new BodyDef();
        var shape = new PolygonShape();
        shape.Set(rect.Vertices());
        var body = world.CreateBody(def);
        body.CreateFixture(new FixtureDef { Shape = shape, Friction = 0 });
    }
}