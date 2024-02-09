using JEngine.util;

namespace JEngine.defs;

public struct DefRef {
    private string id;
    
    public Def? Def => Find.AssetManager.GetDef(id);

    public DefRef(string id) {
        this.id = id;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DefOf : Attribute {}

public static class DefUtility {
    public static void LoadDefOfs() {
        var types = TypeUtility.GetTypesWithAttribute<DefOf>();

        foreach (var type in types) {
            var fields = type.GetFields();

            foreach (var field in fields) {
                if (!typeof(Def).IsAssignableFrom(field.FieldType.BaseType)) {
                    Debug.Warn($"Failed to load DefOf for {field.Name}");
                    continue;
                };
                
                field.SetValue(null, Find.AssetManager.GetDef(field.FieldType, field.Name));
            }
        }
    }
}