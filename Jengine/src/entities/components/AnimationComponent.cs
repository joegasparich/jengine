namespace JEngine.entities;

public class Animation {
    public int StartIndex;
    public int NumFrames;
    public int Speed;
    
    public Animation(int startIndex, int numFrames, int speed) {
        StartIndex = startIndex;
        NumFrames = numFrames;
        Speed = speed;
    }
}

public class AnimationComponent : Component {
    private Dictionary<string, Animation> animations = new();
    
    protected override Type[]          Dependencies => [typeof(RenderComponent)];
    private            RenderComponent Render       => entity.GetComponent<RenderComponent>();
    
    public AnimationComponent(Entity entity, ComponentData? data = null) : base(entity, data) { }
    
    public void AddAnimation(string name, Animation animation) {
        animations.Add(name, animation);
    }
    
    public void PlayAnimation(string name) {
        if (animations.TryGetValue(name, out var animation)) {
            Render.Graphics.SetAnimation(animation.StartIndex, animation.NumFrames, animation.Speed);
        }
    }
}