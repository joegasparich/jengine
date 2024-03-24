using System.Numerics;
using Newtonsoft.Json.Linq;
using JEngine.defs;
using JEngine.util;

namespace JEngine.entities;

public static class EntitySerialiseUtility {
    public static JToken SaveEntities(IEnumerable<Entity?> entities) {
        var parent = Find.SaveManager.CurrentSaveNode;

        var saveData = new JArray();

        foreach (var entity in entities) {
            var entityData = new JObject();
            Find.SaveManager.CurrentSaveNode = entityData;
            entity.Serialise();
            saveData.Add(entityData);
        }

        Find.SaveManager.CurrentSaveNode = parent;
        return saveData;
    }

    public static void LoadEntities(JArray data) {
        var parent = Find.SaveManager.CurrentSaveNode;
        Find.SaveManager.Mode = SerialiseMode.Loading;
        
        foreach (JObject entityData in data) {
            Find.SaveManager.CurrentSaveNode = entityData;
            var type = Type.GetType(entityData["type"].Value<string>());

            Entity entity;

            if (entityData.ContainsKey("def")) {
                // If has def - load from def
                var def  = Find.AssetManager.GetDef(entityData["def"].Value<string>()) as EntityDef;
                entity = Create.CreateEntityForLoad(type, def, entityData["id"].Value<int>());
            } else {
                // Otherwise we need to load the component data from the save file
                entity = Create.CreateEntityForLoad(type, entityData["id"].Value<int>());

                foreach (JObject comp in entityData["components"]) {
                    var compType = TypeUtility.GetTypeByName(comp["type"].Value<string>());
                    if (comp.ContainsKey("data")) {
                        var compDataType = TypeUtility.GetTypeByName(comp["data"]["type"].Value<string>());
                        var compData = (ComponentData) comp["data"]["val"].ToObject(compDataType, Find.SaveManager.Serializer);
                        entity.AddComponent(Activator.CreateInstance(compType, entity, compData) as Component);
                    } else {
                        entity.AddComponent(Activator.CreateInstance(compType, entity, null) as Component);
                    }
                }
            }


            entity.Serialise();
            entity.Setup(true);
        }
        
        Find.SaveManager.CurrentSaveNode = parent;
    }

    public static void ResolveRefsEntities(JArray data, List<Entity?> entities) {
        var parent = Find.SaveManager.CurrentSaveNode;
        
        for (var i = 0; i < entities.Count; i++) {
            var entity = entities[i];
            Find.SaveManager.CurrentSaveNode = data[i] as JObject;
            entity.Serialise();
        }

        Find.SaveManager.CurrentSaveNode = parent;
    }

    public static JToken SaveComponents(IEnumerable<Component> components) {
        var parent = Find.SaveManager.CurrentSaveNode;

        var saveData = new JArray();

        foreach (var component in components) {
            var componentData = new JObject();
            Find.SaveManager.CurrentSaveNode = componentData;
            component.Serialise();
            saveData.Add(componentData);
        }

        Find.SaveManager.CurrentSaveNode = parent;
        return saveData;
    }
    
    public static void LoadComponents(Entity entity, JArray data) {
        var parent = Find.SaveManager.CurrentSaveNode;

        foreach (var entityData in data) {
            Find.SaveManager.CurrentSaveNode = entityData as JObject;
            Type componentType = TypeUtility.GetTypeByName(entityData["type"].Value<string>());
            if (entity.HasComponent(componentType)) {
                var component = entity.GetComponent(componentType);
                component.Serialise();
            } else {
                var component = (Component)Activator.CreateInstance(componentType, entity, null);
                component.Serialise();
                entity.AddComponent(component);
            }
        }
        
        Find.SaveManager.CurrentSaveNode = parent;
    }
    
    public static void ResolveRefsComponents(JArray data, List<Component> components) {
        var parent = Find.SaveManager.CurrentSaveNode;
        for (var i = 0; i < components.Count; i++) {
            var component = components[i];
            Find.SaveManager.CurrentSaveNode = data[i] as JObject;
            component.Serialise();
        }

        Find.SaveManager.CurrentSaveNode = parent;
    }
}