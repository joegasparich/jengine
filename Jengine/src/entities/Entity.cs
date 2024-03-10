using Newtonsoft.Json.Linq;
using JEngine.defs;
using JEngine.util;

namespace JEngine.entities;

public static class EntityTags {
    public const string All = "All";
}

public class Entity : ISerialisable, IReferencable {
    // Config
    public  int                         id;
    private Dictionary<Type, Component> components = new();
    private EntityDef?                  def;
    private HashSet<string>             tags { get; } = new();
    private string?                     name;

    // State
    private Transform transform;
    public  Entity?   parent;

    // Unsaved
    public bool destroyed;
    
    // Properties
    public         IEnumerable<Component> Components => components.Values;
    public virtual EntityDef              Def        => def;
    public virtual string                 Name       => name ?? Def?.name ?? "Unnamed Entity";
    public         RenderComponent        Renderer   => GetComponent<RenderComponent>();
    public         Graphic                Graphic    => GetComponent<RenderComponent>().Graphics;
    public virtual bool                   Selectable => true;
    public         IEnumerable<string>    Tags       => tags;
    public         Transform              Transform  => transform;

    public Entity() {
        transform = new Transform(this);
    }
    public Entity(EntityDef? def) {
        transform = new Transform(this);
        
        this.def = def;

        foreach (var tag in def.Tags) {
            tags.Add(tag);
        }
    }

    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("type", () => GetType().ToString(), null);
        Find.SaveManager.ArchiveValue("id",   ref id);
        Find.SaveManager.ArchiveDeep("transform",  ref transform);

        Find.SaveManager.ArchiveCustom("components",
            () => EntitySerialiseUtility.SaveComponents(components.Values),
            data => EntitySerialiseUtility.LoadComponents(this, data as JArray),
            data => EntitySerialiseUtility.ResolveRefsComponents(data as JArray, components.Values.ToList())
        );
        
        Find.SaveManager.ArchiveDef("def", ref def);
    }

    public virtual void Setup(bool fromSave) {
        foreach (var component in components.Values) {
            component.Setup(fromSave);
        }
    }

    public virtual void PreUpdate() {
        foreach (var component in components.Values) {
            component.PreUpdate();
        }
    }

    public virtual void Update() {
        foreach (var component in components.Values) {
            component.Update();
        }
    }

    public virtual void PostUpdate() {
        foreach (var component in components.Values) {
            component.PostUpdate();
        }
    }

    public virtual void UpdateRare() {
        foreach (var component in components.Values) {
            component.UpdateRare();
        }
    }

    public virtual void Draw() {
        foreach (var component in components.Values) {
            component.Draw();
        }
    }

    public virtual void DrawLate() {
        foreach (var component in components.Values) {
            component.DrawLate();
        }
    }

    public virtual void OnGUI() {
        foreach (var component in components.Values) {
            component.OnGUI();
        }
    }

    public virtual void Destroy() {
        foreach (var component in components.Values) {
            component.End();
        }
        
        destroyed = true;
        
        Find.Game.UnregisterEntity(this);
    }

    public virtual void OnInput(InputEvent evt) {
        foreach (var component in components.Values) {
            component.OnInput(evt);

            if (evt.consumed) 
                return;
        }
    }
    
    // Add existing component
    public T AddComponent<T>(T component) where T : Component {
        components.Add(component.GetType(), component);
        return component;
    }
    // Generate component from type
    public T AddComponent<T>(ComponentData? data = null) where T : Component {
        var component = (T)Activator.CreateInstance(typeof(T), this, data)!;
        components.Add(component.GetType(), component);
        return component;
    }
    public Component AddComponent(Type type, ComponentData? data = null) {
        var component = (Component)Activator.CreateInstance(type, this, data)!;
        components.Add(component.GetType(), component);
        return component;
    }
    
    public T? GetComponent<T>() where T : Component {
        if (!HasComponent(typeof(T))) 
            return null;
        
        if (components.ContainsKey(typeof(T)))
            return (T)components[typeof(T)];
        
        foreach (var type in components.Keys) {
            if (typeof(T).IsAssignableFrom(type))
                return (T)components[type];
        }
        
        return null;
    }
    public Component? GetComponent(Type type) {
        if (!HasComponent(type)) return null;
        
        if (components.ContainsKey(type))
            return components[type];
        
        foreach (var t in components.Keys) {
            if (type.IsAssignableFrom(t)) {
                return components[t];
            }
        }
        
        return null;
    }

    public bool HasComponent<T>() where T : Component {
        if (components.ContainsKey(typeof(T))) 
            return true;
        
        foreach (var t in components.Keys) {
            if (typeof(T).IsAssignableFrom(t))
                return true;
        }

        return false;
    }
    
    public bool HasComponent(Type type) {
        if (components.ContainsKey(type)) 
            return true;
        
        foreach (var t in components.Keys) {
            if (type.IsAssignableFrom(t))
                return true;
        }

        return false;
    }

    public void SetName(string name) {
        this.name = name;
    }
    
    public void AddTag(string tag) {
        tags.Add(tag);
        
        Find.Game.Notify_EntityTagged(this, tag);
    }
    
    public void RemoveTag(string tag) {
        tags.Remove(tag);
        
        Find.Game.Notify_EntityUntagged(this, tag);
    }
    
    public bool HasTag(string tag) {
        return tags.Contains(tag);
    }

    public override string ToString() {
        return Name;
    }

    public string UniqueId => $"entity.{id}";
    public static IReferencable GetByUniqueId(string id) {
        return Find.Game.GetEntityById(Int32.Parse(id.Split(".").Last()));
    }
}