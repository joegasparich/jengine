namespace JEngine.entities;

public class Animation {
    public int  StartIndex;
    public int  NumFrames;
    public int  Duration;
    public bool Loop = true;
    
    public Animation() {}
    public Animation(int startIndex, int numFrames, int duration, bool loop = true) {
        StartIndex = startIndex;
        NumFrames = numFrames;
        Duration = duration;
        Loop = loop;
        
        if (duration < numFrames)
            Debug.Warn("Animation duration is less than number of frames");
    }
}

public class AnimationComponentData : ComponentData {
    public Dictionary<string, Animation> Animations = new();
    public string                        DefaultAnimation;
}

public class AnimationComponent : Component {
    public static Type DataType => typeof(AnimationComponentData);
    
    private       Dictionary<string, Animation> animations = new();
    protected     string                        CurrentAnimation { get; private set; }

    protected override Type[]                 Dependencies => [typeof(RenderComponent)];
    public             AnimationComponentData Data         => (AnimationComponentData)data;
    protected          RenderComponent        Render       => entity.GetComponent<RenderComponent>();
    
    public AnimationComponent(Entity entity, ComponentData? data = null) : base(entity, data) { }

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        foreach (var (name, animation) in Data.Animations)
            AddAnimation(name, animation);
        
        PlayAnimation(Data.DefaultAnimation);
    }

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