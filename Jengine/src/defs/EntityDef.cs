using JEngine.entities;

namespace JEngine.defs;

public class EntityDef : Def {
    public List<ComponentData> components = new();
    public List<string>        Tags       = new();

    public GraphicData? GraphicData => GetComponentData<RenderComponentData>()?.GraphicData;

    // TODO: Cache
    public T? GetComponentData<T>() where T : ComponentData {
        return components.Find(c => c.GetType() == typeof(T)) as T;
    }
}