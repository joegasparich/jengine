using Box2DSharp.Dynamics;
using JEngine;
using JEngine.entities;

namespace Jengine.util;

public static class PhysicsUtility {
    public static Entity? GetEntity(this Body body) {
        return Find.Physics.GetEntityFromBody(body);
    }
    public static Entity? GetEntity(this Fixture fixture) {
        return fixture.Body.GetEntity();
    }

    public static void Destroy(this Body body) {
        Find.Physics.DestroyBody(body);
    }
}