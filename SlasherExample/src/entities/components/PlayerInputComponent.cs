using System.Numerics;
using JEngine;
using JEngine.entities;
using Raylib_cs;

namespace SlasherExample.entities;

public class PlayerInputComponent : InputComponent
{
    public PlayerInputComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Update() {
        inputVector = new Vector2(0, 0);

        if (Find.Input.IsKeyHeld(KeyboardKey.W)) inputVector.Y -= 1;
        if (Find.Input.IsKeyHeld(KeyboardKey.S)) inputVector.Y += 1;
        if (Find.Input.IsKeyHeld(KeyboardKey.A)) inputVector.X -= 1;
        if (Find.Input.IsKeyHeld(KeyboardKey.D)) inputVector.X += 1;
    }
}