namespace JEngine.entities;

public class Animation {
    public int  StartIndex;
    public int  NumFrames;
    public int  Duration;
    public bool Loop;
    
    public Animation(int startIndex, int numFrames, int duration, bool loop = true) {
        StartIndex = startIndex;
        NumFrames = numFrames;
        Duration = duration;
        Loop = loop;
        
        if (duration < numFrames)
            Debug.Warn("Animation duration is less than number of frames");
    }
}

// TODO: AnimationComponentData

public class AnimationComponent : Component {
    private   Dictionary<string, Animation> animations = new();
    protected string                        CurrentAnimation { get; private set; }

    protected override Type[]          Dependencies => [typeof(RenderComponent)];
    protected          RenderComponent Render     => entity.GetComponent<RenderComponent>();
    
    public AnimationComponent(Entity entity, ComponentData? data = null) : base(entity, data) { }
    
    public void AddAnimation(string name, Animation animation) {
        animations.Add(name, animation);
    }
    
    public void PlayAnimation(string name) {
        if (CurrentAnimation == name)
            return;
        
        if (animations.TryGetValue(name, out var animation)) {
            Render.Graphics.SetAnimation(animation.StartIndex, animation.NumFrames, animation.Duration, animation.Loop);
            
            CurrentAnimation = name;
        }
    }
}