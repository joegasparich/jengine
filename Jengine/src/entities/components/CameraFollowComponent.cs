﻿namespace JEngine.entities;

public class CameraFollowComponent : Component {
    public CameraFollowComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void PostUpdate() {
        Find.Renderer.Camera.JumpTo(Entity.Transform.GlobalPosition);
    }
}