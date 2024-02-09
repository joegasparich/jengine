using JEngine;
using JEngine.entities;

namespace SlasherExample.entities;

public class MoveComponent : Component {
    // Config
    public float acceleration = 3f;

    // Properties
    protected override Type[] Dependencies => [typeof(InputComponent), typeof(PhysicsComponent)];
    private InputComponent Input => entity.GetComponent<InputComponent>();
    private PhysicsComponent Physics => entity.GetComponent<PhysicsComponent>();

    public MoveComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Update() {
        Physics.AddForce(Input.inputVector * acceleration);
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("acceleration", ref acceleration);
    }
}