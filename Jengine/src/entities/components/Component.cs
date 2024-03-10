using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JEngine.entities;

/// <summary>
/// Component Data is immutable, and contains component data that comes from a def
/// </summary>
public class ComponentData {
    [JsonProperty]
    private string compClass;

    public virtual Type CompClass => Type.GetType("JEngine.entities." + compClass);
}

public abstract class Component : ISerialisable {
    // References
    protected Entity        entity;
    public    ComponentData data;
    
    // Properties
    public            Entity Entity       => entity;
    protected virtual Type[] Dependencies => Array.Empty<Type>();
    
    public Component(Entity entity, ComponentData? data = null) {
        this.entity = entity;
        this.data   = data;
    }

    public virtual void Setup(bool fromSave) {
        foreach (var dependency in Dependencies) {
            Debug.Assert(entity.HasComponent(dependency), $"Entity {entity} does not have dependency {dependency}");
        }
    }
    public virtual void PreUpdate()             {}
    public virtual void Update()                {}
    public virtual void PostUpdate()            {}
    public virtual void UpdateRare()            {}
    public virtual void Draw()                  {}
    public virtual void DrawLate()              {}
    public virtual void OnGUI()                 {}
    public virtual void OnInput(InputEvent evt) {}
    public virtual void End()                   {}

    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("type", () => GetType().ToString(), null);

        if (entity.Def == null && data != null) {
            Find.SaveManager.ArchiveCustom("data",
                () => {
                    var saveData = new JObject();
                    saveData.Add("type", data.GetType().ToString());
                    saveData.Add("val", JToken.FromObject(data, Find.SaveManager.serializer));
                    return saveData;
                },
                _ => {},
                _ => {}
            );
        }
    }
}