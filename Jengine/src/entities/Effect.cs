using System.Numerics;

namespace JEngine.entities;

public class Effect {
    public Graphic    graphic;
    public Animation? animation;
    public int        duration = -1;
    
    public Entity Spawn(Vector2 pos) {
        var effect = Create.CreateEntity();
        effect.pos = pos;
        var render = effect.AddComponent<RenderComponent>(new RenderComponentData { Graphic = graphic });
        if (animation != null)
            render.Graphics.SetAnimation(animation.StartIndex, animation.NumFrames, animation.Duration, animation.Loop);
        if (duration > 0)
            effect.AddComponent<AutoDestroyComponent>(new AutoDestroyComponentData { Timer = duration });
        
        return effect;
    }
}