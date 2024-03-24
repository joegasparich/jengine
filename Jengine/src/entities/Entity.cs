using Newtonsoft.Json.Linq;
using JEngine.defs;
using JEngine.util;

namespace JEngine.entities;

public static class EntityTags {
    public const string All = "All";
}

public class Entity : ISerialisable, IReferencable {
    // Config
    public           int                         Id;
    private readonly Dictionary<Type, Component> _components = new();
    private          EntityDef?                  _def;
    private          HashSet<string>             _tags = new();
    private          string?                     _name;

    // State
    private Transform _transform;
    public  Entity?   Parent;

    // Unsaved
    public bool Destroyed;
    
    // Properties
    public         IEnumerable<Component> Components => _components.Values;
    public virtual EntityDef?             Def        => _def;
    public virtual string                 Name       => _name ?? Def?.Name ?? "Unnamed Entity";
    public         RenderComponent?       Renderer   => GetComponent<RenderComponent>();
    public         Graphic?               Graphic    => GetComponent<RenderComponent>()?.Graphics;
    public virtual bool                   Selectable => true;
    public         IEnumerable<string>    Tags       => _tags;
    public         Transform              Transform  => _transform;

    public Entity() {
        _transform = new Transform(this);
    }
    public Entity(EntityDef? def) {
        _transform = new Transform(this);
        
        _def = def;

        if (def?.Tags != null) {
            foreach (var tag in def.Tags) {
                _tags.Add(tag);
            }
        }
    }

    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("type", () => GetType().ToString(), null);
        Find.SaveManager.ArchiveValue("id",   ref Id);
        Find.SaveManager.ArchiveDeep("transform",  ref _transform);

        Find.SaveManager.ArchiveCustom("components",
            () => EntitySerialiseUtility.SaveComponents(_components.Values),
            data => EntitySerialiseUtility.LoadComponents(this, data as JArray),
            data => EntitySerialiseUtility.ResolveRefsComponents(data as JArray, _components.Values.ToList())
        );
        
        Find.SaveManager.ArchiveDef("def", ref _def);
    }

    public virtual void Setup(bool fromSave) {
        foreach (var component in _components.Values) {
            component.Setup(fromSave);
        }
    }

    public virtual void PreUpdate() {
        foreach (var component in _components.Values) {
            component.PreUpdate();
        }
    }

    public virtual void Update() {
        foreach (var component in _components.Values) {
            component.Update();
        }
    }

    public virtual void PostUpdate() {
        foreach (var component in _components.Values) {
            component.PostUpdate();
        }
    }

    public virtual void UpdateRare() {
        foreach (var component in _components.Values) {
            component.UpdateRare();
        }
    }

    public virtual void Draw() {
        foreach (var component in _components.Values) {
            component.Draw();
        }
    }

    public virtual void DrawLate() {
        foreach (var component in _components.Values) {
            component.DrawLate();
        }
    }

    public virtual void OnGUI() {
        foreach (var component in _components.Values) {
            component.OnGUI();
        }
    }

    public virtual void Destroy() {
        foreach (var component in _components.Values) {
            component.End();
        }
        
        Destroyed = true;
        
        Find.Game.UnregisterEntity(this);
    }

    public virtual void OnInput(InputEvent evt) {
        foreach (var component in _components.Values) {
            component.OnInput(evt);

            if (evt.Consumed) 
                return;
        }
    }
    
    // Add existing component
    public T AddComponent<T>(T component) where T : Component {
        _components.Add(component.GetType(), component);
        return component;
    }
    // Generate component from type
    public T AddComponent<T>(ComponentData? data = null) where T : Component {
        var component = (T)Activator.CreateInstance(typeof(T), this, data)!;
        _components.Add(component.GetType(), component);
        return component;
    }
    public Component AddComponent(Type type, ComponentData? data = null) {
        var component = (Component)Activator.CreateInstance(type, this, data)!;
        _components.Add(component.GetType(), component);
        return component;
    }
    
    public T? GetComponent<T>() where T : Component {
        if (!HasComponent(typeof(T))) 
            return null;
        
        if (_components.ContainsKey(typeof(T)))
            return (T)_components[typeof(T)];
        
        foreach (var type in _components.Keys) {
            if (typeof(T).IsAssignableFrom(type))
                return (T)_components[type];
        }
        
        return null;
    }
    public Component? GetComponent(Type type) {
        if (!HasComponent(type)) return null;
        
        if (_components.ContainsKey(type))
            return _components[type];
        
        foreach (var t in _components.Keys) {
            if (type.IsAssignableFrom(t)) {
                return _components[t];
            }
        }
        
        return null;
    }

    public bool HasComponent<T>() where T : Component {
        if (_components.ContainsKey(typeof(T))) 
            return true;
        
        foreach (var t in _components.Keys) {
            if (typeof(T).IsAssignableFrom(t))
                return true;
        }

        return false;
    }
    
    public bool HasComponent(Type type) {
        if (_components.ContainsKey(type)) 
            return true;
        
        foreach (var t in _components.Keys) {
            if (type.IsAssignableFrom(t))
                return true;
        }

        return false;
    }

    public void SetName(string name) {
        this._name = name;
    }
    
    public void AddTag(string tag) {
        _tags.Add(tag);
        
        Find.Game.Notify_EntityTagged(this, tag);
    }
    
    public void RemoveTag(string tag) {
        _tags.Remove(tag);
        
        Find.Game.Notify_EntityUntagged(this, tag);
    }
    
    public bool HasTag(string tag) {
        return _tags.Contains(tag);
    }

    public override string ToString() {
        return Name;
    }

    public string UniqueId => $"entity.{Id}";
    public static IReferencable? GetByUniqueId(string id) {
        return Find.Game.GetEntityById(int.Parse(id.Split(".").Last()));
    }
}