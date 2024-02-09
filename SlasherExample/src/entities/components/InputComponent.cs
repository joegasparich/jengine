using System.Numerics;
using JEngine;
using JEngine.entities;

namespace SlasherExample.entities;

public class InputComponent : Component {
    // State
    public Vector2 inputVector;

    public InputComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("inputVector", ref inputVector);
    }
}