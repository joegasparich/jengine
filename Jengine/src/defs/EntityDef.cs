using JEngine.entities;

namespace JEngine.defs;

public class EntityDef : Def {
    public List<ComponentData> Components = new();
    public List<string>        Tags       = new();

    public Graphic? Graphic => GetComponentData<RenderComponentData>()?.Graphic;

    // TODO: Cache
    public T? GetComponentData<T>() where T : ComponentData {
        return Components.Find(c => c.GetType() == typeof(T)) as T;
    }
}