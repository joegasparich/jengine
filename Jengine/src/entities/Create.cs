using JEngine.defs;

namespace JEngine.entities;

public static class Create {
    public static Entity? CreateEntity(int? id = null) {
        try {
            var entity = Activator.CreateInstance<Entity>();

            Find.Game.RegisterEntity(entity, id);

            return entity;
        } catch (Exception e) {
            Debug.Error("Failed to create entity: ", e);
            return null;
        }
    }

    public static Entity? CreateEntity(EntityDef def, int? id = null) {
        try {
            var entity = Activator.CreateInstance(typeof(Entity), def) as Entity;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            Find.Game.RegisterEntity(entity, id);

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create with def {def.Id}: ", e);
            return null;
        }
    }

    public static T? CreateEntity<T>(EntityDef def, int? id = null) where T : Entity {
        try {
            var entity = Activator.CreateInstance(typeof(T), def) as T;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            Find.Game.RegisterEntity(entity, id);

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity of type {typeof(T).Name} with def {def.Id}: ", e);
            return null;
        }
    }
    
    public static Entity? CreateEntity(Type type, EntityDef def, int? id = null) {
        try {
            var entity = Activator.CreateInstance(type, def) as Entity;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            Find.Game.RegisterEntity(entity, id);

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity of type {type.Name} with def {def.Id}: ", e);
            return null;
        }
    }
    
    public static Entity? CreateEntityForLoad(Type type, int id) {
        try {
            var entity = Activator.CreateInstance(type) as Entity;

            Find.Game.RegisterEntityNow(entity, id);

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity of type {type.Name}: ", e);
            return null;
        }
    }

    public static Entity? CreateEntityForLoad(Type type, EntityDef def, int id) {
        try {
            var entity = Activator.CreateInstance(type, def) as Entity;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            Find.Game.RegisterEntityNow(entity, id);

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity of type {type.Name} with def {def.Id}: ", e);
            return null;
        }
    }
}