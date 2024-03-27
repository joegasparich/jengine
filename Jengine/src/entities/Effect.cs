using System.Numerics;

namespace JEngine.entities;

public class Effect {
    public Graphic    Graphic;
    public Animation? Animation;
    public int        Duration = -1;
    
    public Entity? Spawn(Vector2 pos, float rotation, Entity? parent = null) {
        var effect = Create.CreateEntity();
        effect.Transform.LocalPosition = pos;
        effect.Transform.LocalRotation = rotation;
        var render = effect.AddComponent<RenderComponent>(new RenderComponentData { Graphic = Graphic });
        if (Animation != null)
            render.Graphics.SetAnimation(Animation.StartIndex, Animation.NumFrames, Animation.Duration, Animation.Loop);
        if (Duration > 0)
            effect.AddComponent<AutoDestroyComponent>(new AutoDestroyComponentData { Timer = Duration });
        if (parent != null)
            effect.Parent = parent;
        
        effect.Setup(false);
        
        return effect;
    }
}