using JEngine.entities;
using JEngine.util;

namespace SlasherExample.entities;

public class EnemyAnimationComponent : AnimationComponent {
    public const string Idle = "Idle";
    public const string Walk = "Walk";
    
    protected override Type[] Dependencies => [typeof(RenderComponent), typeof(InputComponent)];
    
    private InputComponent Input => entity.GetComponent<InputComponent>();
    
    public EnemyAnimationComponent(Entity entity, ComponentData? data = null) : base(entity, data) { }

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        PlayAnimation(Idle);
    }

    public override void PostUpdate() {
        base.PostUpdate();
        
        if (Input.inputVector.Magnitude() > 0.01f)
            PlayAnimation(Walk);
        else
            PlayAnimation(Idle);
        
        Render.Graphics.flipX = Input.inputVector.X < 0;
    }
}