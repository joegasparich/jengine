using JEngine.util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JEngine.entities;

/// <summary>
/// Component Data is immutable, and contains component data that comes from a def
/// </summary>
public class ComponentData {
    [JsonProperty]
    private string _compClass;

    public virtual Type CompClass => TypeUtility.GetTypeByName(_compClass);
}

public abstract class Component : ISerialisable {
    // References
    public readonly    Entity         Entity;
    protected readonly ComponentData? _data;
    
    // Properties
    protected virtual Type[] Dependencies => Array.Empty<Type>();
    
    public Component(Entity entity, ComponentData? data = null) {
        Entity = entity;
        _data   = data;
        
        if (_data == null && GetType().GetProperty("DataType")?.GetValue(null) is Type type)
            _data = Activator.CreateInstance(type) as ComponentData;
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

        if (Entity.Def == null && _data != null) {
            Find.SaveManager.ArchiveCustom("data",
                () => {
                    var saveData = new JObject();
                    saveData.Add("type", _data.GetType().ToString());
                    saveData.Add("val", JToken.FromObject(_data, Find.SaveManager.Serializer));
                    return saveData;
                },
                _ => {},
                _ => {}
            );
        }
    }
}