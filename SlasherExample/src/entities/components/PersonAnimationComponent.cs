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
    private RenderComponent Render => entity.GetComponent<RenderComponent>();
    private InputComponent Input => entity.GetComponent<InputComponent>();

    public PersonAnimationComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

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
        var baseIndex = moving ? WalkIndex : IdleIndex;

        if (dir == Dir4.East || dir == Dir4.West)
            Render.Graphics.SetAnimation(baseIndex + RightIndex, AnimationFrames, AnimationSpeed);
        if (dir == Dir4.South)
            Render.Graphics.SetAnimation(baseIndex + DownIndex, AnimationFrames, AnimationSpeed);
        if (dir == Dir4.North)
            Render.Graphics.SetAnimation(baseIndex + UpIndex, AnimationFrames, AnimationSpeed);
    }
}