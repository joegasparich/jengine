using System.Numerics;
using JEngine;
using JEngine.entities;
using JEngine.util;
using Raylib_cs;

namespace SlasherExample.entities;

public class PlayerInputComponent : InputComponent
{
    private Graphic Cursor = new("cursor.png");
    public PlayerInputComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    private Vector2 AimVector => (Find.Input.GetMouseWorldPos() - entity.pos).Normalised();

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

    public override void Draw() {
        base.Draw();

        Cursor.Draw(
            entity.pos,
            rotation: AimVector.Angle() + 90,
            depth: (int)Depth.UI,
            overrideColour: Color.White.WithAlpha(0.5f)
        );
    }
}