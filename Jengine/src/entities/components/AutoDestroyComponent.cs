namespace JEngine.entities;

public class AutoDestroyComponentData : ComponentData {
    public int Timer;
}

public class AutoDestroyComponent(Entity entity, ComponentData? data = null) : Component(entity, data) {
    public static Type DataType => typeof(AutoDestroyComponentData);
    
    public int Timer;
    
    public AutoDestroyComponentData Data => (AutoDestroyComponentData)_data;

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        Timer = Data.Timer;
    }

    public override void PreUpdate() {
        if (Timer > 0)
            Timer--;
        else
            Entity.Destroy();
    }
}