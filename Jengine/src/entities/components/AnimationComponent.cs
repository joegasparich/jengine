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

public class AnimationComponent(Entity entity, ComponentData? data = null) : Component(entity, data) {
    public static Type DataType => typeof(AnimationComponentData);

    private   Dictionary<string, Animation> _animations = new();
    protected string?                       CurrentAnimation { get; private set; }

    protected override Type[]                  Dependencies => [typeof(RenderComponent)];
    public             AnimationComponentData? Data         => _data as AnimationComponentData;
    protected          RenderComponent         Render       => Entity.GetComponent<RenderComponent>()!;

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        foreach (var (name, animation) in Data.Animations)
            AddAnimation(name, animation);
        
        PlayAnimation(Data.DefaultAnimation);
    }

    public void AddAnimation(string name, Animation animation) {
        _animations.Add(name, animation);
    }
    
    public void PlayAnimation(string name) {
        if (CurrentAnimation == name)
            return;
        
        if (_animations.TryGetValue(name, out var animation)) {
            Render.Graphics?.SetAnimation(animation.StartIndex, animation.NumFrames, animation.Duration, animation.Loop);
            
            CurrentAnimation = name;
        }
    }
}