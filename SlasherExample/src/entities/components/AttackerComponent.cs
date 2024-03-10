using System.Numerics;
using Box2DSharp.Collision;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using JEngine;
using JEngine.entities;
using JEngine.util;
using Raylib_cs;

namespace SlasherExample.entities;

public class AttackerComponent : Component {
    private PolygonShape hitbox;

    class HitCallback : IQueryCallback {
        private Entity       entity;
        private PolygonShape hitbox;
        private float        angle;

        public HitCallback(Entity entity, PolygonShape hitbox, float angle) {
            this.entity = entity;
            this.hitbox = hitbox;
            this.angle  = angle;
        }

        public bool QueryCallback(Fixture fixture) {
            if (!CollisionUtils.TestOverlap(hitbox, 0, fixture.Shape, 0, new Box2DSharp.Common.Transform(entity.Transform.GlobalPosition, angle * JMath.DegToRad), fixture.Body.GetTransform(), null))
                return true;

            var hitEntity = Find.Game.physics.GetEntityFromBody(fixture.Body);

            if (hitEntity == null)
                return true;

            if (hitEntity == entity)
                return true;

            hitEntity.GetComponent<PhysicsComponent>()?.AddForce(new Vector2(0, 0.05f).Rotate(angle * JMath.DegToRad));

            return true;
        }
    }

    public AttackerComponent(Entity entity, ComponentData? data = null) : base(entity, data) {
        hitbox = new PolygonShape();
        hitbox.Set([
            new(-0.5f, 0),
            new(-0.5f, 1.5f),
            new(0.5f,  1.5f),
            new(0.5f,  0)
        ]);
    }

    public void DoAttack(float angle) {
        hitbox.ComputeAABB(out var aabb, new Box2DSharp.Common.Transform(entity.Transform.GlobalPosition, (angle - 90) * JMath.DegToRad), 0);

        Find.Game.physics.world.QueryAABB(new HitCallback(entity, hitbox, angle - 90), aabb);
    }

    // public override void DrawLate() {
    //     var angle = (Find.Input.GetMouseWorldPos() - entity.Transform.GlobalPosition).AngleDeg();
    //
    //     // hitbox.ComputeAABB(out var aabb, new Box2DSharp.Common.Transform(entity.Transform.GlobalPosition, (angle - 90) * JMath.DegToRad), 0);
    //     // Drawing.DrawRectangleOutline(new Rectangle(aabb.LowerBound * Find.Config.worldScalePx, (aabb.UpperBound - aabb.LowerBound) * Find.Config.worldScalePx), Color.Red, (int)Depth.Debug);
    //
    //     Drawing.DrawLineStrip(hitbox.Vertices.Select(v => (v.RotateAround(Vector2.Zero, angle - 90) + entity.Transform.GlobalPosition) * Find.Config.worldScalePx).Take(hitbox.Count).ToArray(), Color.Red, (int)Depth.Debug);
    // }
}