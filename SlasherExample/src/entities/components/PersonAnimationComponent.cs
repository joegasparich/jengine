using System.Numerics;
using JEngine.entities;
using JEngine.util;

namespace SlasherExample.entities;

public class PersonAnimationComponent : AnimationComponent
{
    private const int AnimationFrames = 6;
    private const int IdleIndex       = 0;
    private const int WalkIndex       = 18;
    private const int DownIndex       = 0;
    private const int RightIndex      = 6;
    private const int UpIndex         = 12;
    private const int AnimationSpeed  = 48;

    private Dir4 direction = Dir4.South;

    protected override Type[] Dependencies => [typeof(RenderComponent), typeof(InputComponent)];
    private InputComponent Input => entity.GetComponent<InputComponent>();

    public PersonAnimationComponent(Entity entity, ComponentData? data = null) : base(entity, data) {
        AddAnimation("IdleRight", new Animation(IdleIndex + RightIndex, AnimationFrames, AnimationSpeed));
        AddAnimation("IdleUp", new Animation(IdleIndex + UpIndex, AnimationFrames, AnimationSpeed));
        AddAnimation("IdleDown", new Animation(IdleIndex + DownIndex, AnimationFrames, AnimationSpeed));
        AddAnimation("WalkRight", new Animation(WalkIndex + RightIndex, AnimationFrames, AnimationSpeed));
        AddAnimation("WalkUp", new Animation(WalkIndex + UpIndex, AnimationFrames, AnimationSpeed));
        AddAnimation("WalkDown", new Animation(WalkIndex + DownIndex, AnimationFrames, AnimationSpeed));
    }

    public override void PostUpdate() {
        if (MathF.Abs(Input.inputVector.X) >= MathF.Abs(Input.inputVector.Y)) {
            if (Input.inputVector.X > 0)
                direction = Dir4.East;
            else if (Input.inputVector.X < 0)
                direction = Dir4.West;
        } else {
            if (Input.inputVector.Y > 0)
                direction = Dir4.South;
            else if (Input.inputVector.Y < 0)
                direction = Dir4.North;
        }

        SetAnimation(direction, Input.inputVector != Vector2.Zero);
        
        Render.Graphics.flipX = direction == Dir4.West;
    }

    private void SetAnimation(Dir4 dir, bool moving) {
        if (dir == Dir4.East || dir == Dir4.West)
            PlayAnimation(moving ? "WalkRight" : "IdleRight");
        if (dir == Dir4.South)
            PlayAnimation(moving ? "WalkDown" : "IdleDown");
        if (dir == Dir4.North)
            PlayAnimation(moving ? "WalkUp" : "IdleUp");
    }
}