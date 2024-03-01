using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using JEngine.util;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace JEngine;

public class PhysicsManager {
    public World world;

    private float msPerUpdate;

    public void Init() {
        msPerUpdate = (float)Find.Game.gameConfig.msPerUpdate;

        world = new World(Vector2.Zero);
    }

    public void Update() {
        world.Step(msPerUpdate, 6, 3);
    }

    public void RenderLate() {
        // DrawDebug();
    }

    private void DrawDebug() {
        var body = world.GetBodyList();
        while (body != null) {
            var fixture = body.GetFixtureList();
            if (fixture.Shape is CircleShape circle) {
                Debug.DrawCircleLines(body.Position * Find.Game.gameConfig.worldScalePx, circle.Radius * Find.Game.gameConfig.worldScalePx, Color.Red);
            } else if (fixture.Shape is PolygonShape poly) {
                var vertices = poly.GetVertices();
                for (var i = 0; i < vertices.Length; i++) {
                    var vertex     = vertices[i];
                    var nextVertex = vertices[(i + 1) % vertices.Length];
                    Debug.DrawLine(
                        (body.Position + vertex) * Find.Game.gameConfig.worldScalePx,
                        (body.Position + nextVertex) * Find.Game.gameConfig.worldScalePx,
                        Color.Red
                    );
                }
            }

            body = body.GetNext();
        };
    }

    public void RegisterCollider(Rectangle rect) {
        var def   = new BodyDef();
        var shape = new PolygonShape();
        shape.Set(rect.Vertices());
        var body = world.CreateBody(def);
        body.CreateFixture(shape);
    }
}