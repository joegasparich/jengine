namespace JEngine.entities;

public class AutoDestroyComponentData : ComponentData {
    public int Timer;
}

public class AutoDestroyComponent : Component {
    public static Type DataType => typeof(AutoDestroyComponentData);
    
    public int timer;
    
    public AutoDestroyComponentData Data => (AutoDestroyComponentData)data;
    
    public AutoDestroyComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        timer = Data.Timer;
    }

    public override void PreUpdate() {
        if (timer > 0)
            timer--;
        else
            entity.Destroy();
    }
}