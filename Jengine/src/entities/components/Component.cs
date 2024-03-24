using JEngine.util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JEngine.entities;

/// <summary>
/// Component Data is immutable, and contains component data that comes from a def
/// </summary>
public class ComponentData {
    [JsonProperty]
    private string compClass;

    public virtual Type CompClass => TypeUtility.GetTypeByName(compClass);
}

public abstract class Component : ISerialisable {
    // References
    public readonly    Entity         Entity;
    protected readonly ComponentData? data;
    
    // Properties
    protected virtual Type[] Dependencies => Array.Empty<Type>();
    
    public Component(Entity entity, ComponentData? data = null) {
        Entity    = entity;
        this.data = data;
        
        if (this.data == null && GetType().GetPropertyInherited("DataType")?.GetValue(null) is Type type)
            this.data = Activator.CreateInstance(type) as ComponentData;
    }

    public virtual void Setup(bool fromSave) {
        foreach (var dependency in Dependencies) {
            Debug.Assert(Entity.HasComponent(dependency), $"Entity {Entity} does not have dependency {dependency}");
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

        if (Entity.Def == null && data != null) {
            Find.SaveManager.ArchiveCustom("data",
                () => {
                    var saveData = new JObject();
                    saveData.Add("type", data.GetType().ToString());
                    saveData.Add("val", JToken.FromObject(data, Find.SaveManager.Serializer));
                    return saveData;
                },
                _ => {},
                _ => {}
            );
        }
    }
}