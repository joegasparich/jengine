using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using JEngine.defs;
using JEngine.entities;
using JEngine.util;

namespace JEngine;

// -- Contract Resolver --//

public class CustomContractResolver : DefaultContractResolver {
    protected override JsonContract CreateContract(Type objectType) {
        var contract = base.CreateContract(objectType);
        if (contract is JsonObjectContract objectContract) {
            objectContract.OnPropertySetter = (o, value, property, reader) => {
                if (reader.Depth > 0 && value.GetType().IsAssignableTo(typeof(Def))) {
                    Find.AssetManager.UnresolvedDefs.Add((o, property.ValueProvider as ExpressionValueProvider)!);
                }
            };
        }
        
        return contract;
    }
}

//-- Converters --//

public class DefConverter : JsonConverter {
    private bool skipOverMe;

    public override bool CanConvert(Type objectType) {
        return objectType.IsAssignableTo(typeof(Def));
    }

    public override bool CanRead {
        get {
            if (!skipOverMe) 
                return true;

            skipOverMe = false;
            return false;

        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        // Skip over root level Defs so they can be deserialized normally
        if (reader.Depth == 0) {
            skipOverMe = true;
            return serializer.Deserialize(reader, objectType);
        }

        // Defs already exist when loading save
        if (Find.AssetManager.GetDef(reader.Value as string, suppressError: true) is Def existingDef)
            return existingDef;

        // Otherwise we need to create some unresolved defs
        var def = Activator.CreateInstance(objectType) as Def;
        def.Id       = reader.Value as string;
        def.Resolved = false;

        // if (target != null)
        //     Find.AssetManager.UnresolvedDefs.Add((target, prop.ValueProvider as ExpressionValueProvider)!);

        return def;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        var def = value as Def;
        
        writer.WriteValue(def?.Id);
    }
}

public class TypeConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return objectType.IsAssignableTo(typeof(Type));
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        var type = TypeUtility.GetTypeByName(reader.Value as string);
        
        if (type == null)
            Debug.Warn("Type not found: " + reader.Value);
        
        return type;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        var type = value as Type;
        
        writer.WriteValue(type.Name);
    }
}

public class CompJsonConverter : JsonConverter {
    // This needs to be kept in sync with the main serializer config but without itself
    private readonly JsonSerializer internalSerializer = JsonSerializer.Create(new JsonSerializerSettings() {
        ContractResolver = new CustomContractResolver(),
        Converters = new List<JsonConverter> {
            new DefConverter(),
            new TypeConverter(),
            new LerpPointsConverter()
        },
    });

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(List<ComponentData>);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        var tokenReader = reader as JTokenReader;
        var token       = tokenReader.CurrentToken;
        var components  = new List<ComponentData>();

        foreach (JProperty child in token) {
            var compType = TypeUtility.GetTypeByName(child.Name);

            if (!compType.IsAssignableTo(typeof(Component))) {
                Debug.Warn("Component type not found: " + child.Name);
                continue;
            }

            var compDataType = compType.GetProperty("DataType")?.GetValue(null) as Type;

            var searchType = compType;
            while (compDataType == null && searchType.BaseType != null) {
                searchType   = searchType.BaseType;
                compDataType = searchType.GetProperty("DataType")?.GetValue(null) as Type;
            }

            ComponentData componentData;
            if (compDataType != null)
                componentData = child.Value.ToObject(compDataType, internalSerializer) as ComponentData;
            else
                componentData = child.Value.ToObject(typeof(ComponentData), internalSerializer) as ComponentData;
            
            var compClassProp = typeof(ComponentData).GetField("compClass", BindingFlags.NonPublic | BindingFlags.Instance);
            compClassProp.SetValue(componentData, child.Name);
            components.Add(componentData);
        }

        reader.Skip();
        return components;
    }

    public override bool CanWrite => false;
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}

public class LerpPointsConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return objectType.IsAssignableTo(typeof(LerpPoints));
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        var curToken = typeof(JTokenReader).GetProperty("CurrentToken")?.GetValue(reader);
        var arrays   = (curToken as JToken)?.ToObject<List<List<float>>>();
        
        reader.Skip();
        
        return new LerpPoints(arrays.Select(p => (p[0], p[1])));
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        var lp = value as LerpPoints;
        var array = lp.Points.Select(p => new List<float> { p.Item1, p.Item2 });
        
        writer.WriteValue(array);
    }
}