namespace JEngine.entities;

public class CameraFollowComponent : Component {
    public CameraFollowComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void PostUpdate() {
        Find.Renderer.camera.JumpTo(entity.Transform.GlobalPosition);
    }
}