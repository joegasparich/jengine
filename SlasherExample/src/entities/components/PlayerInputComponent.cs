using System.Numerics;
using JEngine;
using JEngine.entities;
using JEngine.util;
using Raylib_cs;

namespace SlasherExample.entities;

public class PlayerInputComponent : InputComponent
{
    private readonly Graphic Cursor = new("cursor.png");
    private readonly Effect  SlashEffect = new() {
        graphic = new Graphic("slash.png", 16, 16) {
            origin = new Vector2(0.5f)
        },
        animation = new Animation(0, 3, 12, loop: false),
        duration = 12
    };
    
    private Vector2 AimVector => (Find.Input.GetMouseWorldPos() - entity.Transform.GlobalPosition).Normalised();

    public PlayerInputComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);

        Cursor.origin = new Vector2(0.5f);
    }

    public override void Update() {
        inputVector = new Vector2(0, 0);

        if (Find.Input.IsKeyHeld(KeyboardKey.W)) inputVector.Y -= 1;
        if (Find.Input.IsKeyHeld(KeyboardKey.S)) inputVector.Y += 1;
        if (Find.Input.IsKeyHeld(KeyboardKey.A)) inputVector.X -= 1;
        if (Find.Input.IsKeyHeld(KeyboardKey.D)) inputVector.X += 1;
    }

    public override void OnInput(InputEvent evt) {
        if (evt.consumed)
            return;

        if (evt.mouseDown == MouseButton.Left) {
            var angle = AimVector.AngleDeg();
            SlashEffect.Spawn(new Vector2(1.5f, 0).RotateAround(Vector2.Zero, angle), angle, parent: entity);
            evt.Consume();
        }
    }

    public override void Draw() {
        base.Draw();

        Cursor.Draw(
            entity.Transform.GlobalPosition,
            rotation: AimVector.AngleDeg() + 90,
            depth: (int)Depth.UI,
            overrideColour: Color.White.WithAlpha(0.5f)
        );
    }
}