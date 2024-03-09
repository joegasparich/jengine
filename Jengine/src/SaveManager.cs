using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JEngine.defs;
using JEngine.util;

namespace JEngine;

public interface ISerialisable {
    public void Serialise();
}
public interface IReferencable {
    string                        UniqueId { get; }
    static abstract IReferencable GetByUniqueId(string id);
}

public enum SerialiseMode {
    None,
    Saving,
    Loading,
    ResolvingRefs
}

public enum SaveMode {
    Value,
    Ref,
    Def,
    Deep
}

public class SaveFile {
    public string name;
    public string path;
}

public class SaveManager {
    // Constants
    private const string SaveDir         = "saves/";
    private const string DefaultSaveName = "save";
    public readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings() {
        Converters = new List<JsonConverter> {
            new DefConverter(),
            new CompJsonConverter(),
            new TypeConverter(),
            new LerpPointsConverter(),
        },
    });
    
    // State
    public JObject       currentSaveNode;
    public SerialiseMode mode;

    private Dictionary<ISerialisable, string> DeepSavedObjects = new();

    public void SaveCurrentScene(string name = DefaultSaveName, bool overwrite = false) {
        Debug.Log("Saving game");

        DeepSavedObjects.Clear();

        var saveData = new JObject();
        saveData.Add("saveName", name);
        mode            = SerialiseMode.Saving;
        currentSaveNode = saveData;

        try {
            Find.Game.Serialise();
        } catch (Exception e) {
            Debug.Error("Error saving game: ", e);

            return;
        }

        mode = SerialiseMode.None;

        // Create save folder if it doesn't exist
        if (!Directory.Exists(SaveDir))
            Directory.CreateDirectory(SaveDir);

        var fileName = name.ToSnakeCase();

        if (!overwrite) {
            var postFix = 1;
            while (File.Exists($"{SaveDir}{fileName}.json")) {
                fileName = $"{name.ToSnakeCase()}_{postFix}";
                postFix++;
            }
        }

        // Save json object to file
        var jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText($"{SaveDir}{fileName}.json", jsonString);
    }

    public void LoadIntoCurrentScene(string filePath) {
        Debug.Log("Loading game");

        var json = File.ReadAllText("saves/" + filePath + ".json");
        var saveData = JsonConvert.DeserializeObject<JObject>(json);

        Find.Game.ClearEntities();

        currentSaveNode = saveData;
        try {
            mode = SerialiseMode.Loading;
            Find.Game.Serialise();
            mode = SerialiseMode.ResolvingRefs;
            Find.Game.Serialise();
        } catch (Exception e) {
            Debug.Error("Error loading game: ", e);
        }

        mode = SerialiseMode.None;
    }

    public IEnumerable<SaveFile> GetSaveFiles() {
        if (!Directory.Exists(SaveDir))
            Directory.CreateDirectory(SaveDir);
        
        var files = Directory.GetFiles(SaveDir, "*.json");
        return files.ToList()
            .OrderByDescending(File.GetLastWriteTime)
            .Select(f => new SaveFile {
                name = Path.GetFileNameWithoutExtension(f),
                path = f
            });
    }

    // TODO: Allow serialising regular values
    public JObject Serialise(ISerialisable value) {
        var node = new JObject();
        
        mode            = SerialiseMode.Saving;
        currentSaveNode = node;
        
        value.Serialise();
        return node;
    }

    // Serialise basic type
    // This just uses Newtonsoft.JSON to handle the type
    public void ArchiveValue<T>(string label, ref T? value) {
        if (value is ISerialisable)
            Debug.Warn(label + " is ISerialisable, use ArchiveDeep instead");
        else if (value is IReferencable)
            Debug.Warn(label + " is IReferencable, use ArchiveRef instead");

        switch (mode) {
            case SerialiseMode.Saving:
                if (value == null) 
                    break;
                
                currentSaveNode.Add(label, JToken.FromObject(value, serializer));
                break;
            case SerialiseMode.Loading:
                if (currentSaveNode[label] == null)
                    break;
                
                value = currentSaveNode[label]!.ToObject<T>(serializer);
                break;
        }
    }

    // Serialise basic type with custom getter/setter
    // T must be serialisable by Newtonsoft.JSON
    // Currently used for classNames on entities/components
    // And anything too complicated for the other archivers
    public void ArchiveValue<T>(string label, Func<T> get, Action<T?>? set) {
        switch (mode) {
            case SerialiseMode.Saving: {
                var value = get();
                ArchiveValue(label, ref value);
                break;
            }
            case SerialiseMode.Loading: {
                if (set == null) break;
                
                var value = default(T);
                ArchiveValue(label, ref value);
                set(value);
                break;
            }
        }
    }

    // Serialise def reference using id
    public void ArchiveDef<T>(string label, ref T def) where T : Def {
        switch (mode) {
            case SerialiseMode.Saving: {
                if (def == null) 
                    break;
                
                currentSaveNode.Add(label, JToken.FromObject(def.id, serializer));
                break;
            }
            case SerialiseMode.Loading: {
                if (currentSaveNode[label] == null)
                    break;
                
                var id = currentSaveNode[label]!.Value<string>();
                def = Find.AssetManager.GetDef<T>(id);
                break;
            }
        }
    }
    
    // Serialise a reference using IReferencable interface
    // Implementers must provide a unique ID and a way to get a reference to the same
    // object from that ID
    public void ArchiveRef<T>(string label, ref T value) where T : class, IReferencable {
        switch (mode) {
            case SerialiseMode.Saving:
                if (value == null)
                    break;
                
                currentSaveNode[label] = value.UniqueId;
                break;
            case SerialiseMode.ResolvingRefs:
                if (currentSaveNode[label] == null)
                    break;
                
                var id = currentSaveNode[label]!.Value<string>();
                value = T.GetByUniqueId(id) as T;
                break;
        }
    }
    
    // Serialise ISerialisable, must have empty constructor
    // We can't use the new() constraint because we are using reflection to instantiate
    public void ArchiveDeep<T>(string label, ref T? value, params object[] ctorArgs) where T : class, ISerialisable {
        // We need to store the parent because we are going to recurse
        // and need to restore back to the parent node once we're done
        var parent = currentSaveNode;
        switch (mode) {
            case SerialiseMode.Saving:
                if (value == null)
                    break;
                if (DeepSavedObjects.TryGetValue(value, out var existingPath)) {
                    Debug.Warn($"{currentSaveNode.Path}{label} was already deep saved at {existingPath}");
                    break;
                }
                
                // Create a new node and serialise the value into it
                currentSaveNode              = new JObject();
                currentSaveNode["className"] = value.GetType().ToString(); 
                value.Serialise();
                DeepSavedObjects.Add(value, currentSaveNode.Path);
                parent[label] = currentSaveNode;
                break;
            case SerialiseMode.Loading:
                if (currentSaveNode[label] == null)
                    break;
                
                currentSaveNode = parent[label] as JObject;
                // Instantiate object and serialise into it
                if (value == null) {
                    // Need to use reflection to instantiate because T could be abstract
                    // We need to get the type from the className property
                    var type = Type.GetType(currentSaveNode["className"].Value<string>());
                    if (type == null || type.GetConstructor(Type.EmptyTypes) == null) {
                        Debug.Warn($"Failed to load {label}, type {type} not found or has no default constructor");
                        break;
                    }
                    value = (T) Activator.CreateInstance(type, ctorArgs)!;
                }
                value.Serialise();
                break;
            case SerialiseMode.ResolvingRefs:
                currentSaveNode = parent[label] as JObject;
                if (value == null) {
                    Debug.Warn($"Could not find node {parent.Path}.{label} in save file");
                    break;
                }
                
                value.Serialise();
                break;
        }
        // Restore parent node
        currentSaveNode = parent;
    }
    
    // Serialise ISerialisable - used rarely if the value is a property
    // Value must be already instantiated when loading
    // Above overload is preferred
    public void ArchiveDeep(string label, ISerialisable value) {
        var parent = currentSaveNode;
        switch (mode) {
            case SerialiseMode.Saving:
                if (value == null)
                    break;
                if (DeepSavedObjects.TryGetValue(value, out var existingPath)) {
                    Debug.Warn($"{currentSaveNode.Path}{label} was already deep saved at {existingPath}");
                    break;
                }

                currentSaveNode = new JObject();
                currentSaveNode["className"] = value.GetType().ToString(); 
                value.Serialise();
                DeepSavedObjects.Add(value, currentSaveNode.Path);
                parent[label] = currentSaveNode;
                break;
            case SerialiseMode.Loading:
                if (currentSaveNode[label] == null)
                    break;
                
                if (value == null) {
                    Debug.Warn($"Failed to load {label}, value because it is null");
                    break;
                }
                currentSaveNode = parent[label] as JObject;
                value.Serialise();
                break;
            case SerialiseMode.ResolvingRefs:
                currentSaveNode = parent[label] as JObject;
                value.Serialise();
                break;
        }
        currentSaveNode = parent;
    }

    // Stores a collection of values using any of the four save modes implemented above
    public void ArchiveCollection<T>(string label, ref List<T> collection, SaveMode saveMode) where T : class, new() {
        var parent = currentSaveNode;
        switch (mode) {
            case SerialiseMode.Saving: {
                // Create a json array and generate JTokens to store in it
                var array = new JArray();
                foreach (var value in collection) {
                    switch (saveMode) {
                        case SaveMode.Value:
                            array.Add(JToken.FromObject(value, serializer));
                            break;
                        case SaveMode.Def:
                            var def = value as Def;
                            array.Add(JToken.FromObject(def.id, serializer));
                            break;
                        case SaveMode.Ref:
                            var reference = value as IReferencable;
                            array.Add(JToken.FromObject(reference.UniqueId, serializer));
                            break;
                        case SaveMode.Deep:
                            currentSaveNode = new JObject();
                            array.Add(currentSaveNode);
                            
                            var serialisable = value as ISerialisable;
                            
                            if (DeepSavedObjects.TryGetValue(serialisable, out var existingPath)) {
                                Debug.Warn($"{currentSaveNode.Path}{label} was already deep saved at {existingPath}");
                                break;
                            }
                            
                            serialisable.Serialise();
                            DeepSavedObjects.Add(serialisable, currentSaveNode.Path);
                            break;
                    }
                }
                parent[label] = array;
                break;
            }
            case SerialiseMode.Loading: {
                var array = parent[label] as JArray;
                var type  = typeof(T);

                // Instantiate collection if it does not exist
                if (collection == null)
                    collection = new List<T>();
                
                // Instantiate objects and store in array
                foreach (var value in array) {
                    switch (saveMode) {
                        case SaveMode.Value:
                            collection.Add(value.ToObject<T>(serializer));
                            break;
                        case SaveMode.Def:
                            collection.Add(Find.AssetManager.GetDef(type, value.Value<string>()) as T);
                            break;
                        case SaveMode.Deep:
                            currentSaveNode = value as JObject; 
                            var item = new T() as ISerialisable;
                            item.Serialise();
                            collection.Add(item as T);
                            break;
                    }
                }
                break;
            }
            case SerialiseMode.ResolvingRefs: {
                var array = parent[label] as JArray;
                var type  = typeof(T);

                switch (saveMode) {
                    case SaveMode.Ref: {
                        foreach (var value in array) {
                            var id  = value.Value<string>();
                            var val = type.GetMethod("GetByUniqueId").Invoke(null, new[] { id });
                            collection.Add(val as T);
                        }
                        break;
                    }
                    case SaveMode.Deep: {
                        foreach (var item in collection) {
                            var serialisable = item as ISerialisable;
                            serialisable.Serialise();
                        }
                        break;
                    }
                }
                break;
            }
        }
        currentSaveNode = parent;
    }

    // Serialise IEnumerable of ISerialisable with getter/selector
    // Currently the only method to serialise collections that aren't lists
    // Takes an IEnumerable and a selector function that returns an IEnumerable of ISerialisable
    // Implementers must deserialise the JArray themselves
   public void ArchiveCollection(string label, IEnumerable<ISerialisable> collection, Func<JArray, IEnumerable<ISerialisable>> select) {
        var parent = currentSaveNode;
        switch (mode) {
            case SerialiseMode.Saving: {
                var array = new JArray();
                foreach (var value in collection) {
                    currentSaveNode = new JObject();
                    array.Add(currentSaveNode);
                    
                    if (DeepSavedObjects.TryGetValue(value, out var existingPath)) {
                        Debug.Warn($"{currentSaveNode.Path}{label} was already deep saved at {existingPath}");
                        break;
                    }
                    
                    value.Serialise();
                    DeepSavedObjects.Add(value, currentSaveNode.Path);
                }
                parent[label] = array;
                break;
            }
            case SerialiseMode.Loading: {
                var array = parent[label] as JArray;
                var i = 0;
                foreach (var value in select(array)) {
                    currentSaveNode = array[i++] as JObject;
                    value.Serialise();
                }
                break;
            }
            case SerialiseMode.ResolvingRefs: {
                foreach (var item in collection) {
                    item.Serialise();
                }
                break;
            }
        }
        currentSaveNode = parent;
    }
   
   public void ArchiveCollection<T>(string label, IEnumerable<T> collection, Action<T> select) where T : class, ISerialisable, new() {
       var parent = currentSaveNode;
       switch (mode) {
           case SerialiseMode.Saving: {
               var array = new JArray();
               foreach (var value in collection) {
                   currentSaveNode = new JObject();
                   array.Add(currentSaveNode);
                   
                   if (DeepSavedObjects.TryGetValue(value, out var existingPath)) {
                       Debug.Warn($"{currentSaveNode.Path}{label} was already deep saved at {existingPath}");
                       break;
                   }
                   
                   value.Serialise();
                   DeepSavedObjects.Add(value, currentSaveNode.Path);
               }
               parent[label] = array;
               break;
           }
           case SerialiseMode.Loading: {
               var array = parent[label] as JArray;
               foreach (var value in array) {
                   currentSaveNode = value as JObject; 
                   var item = new T() as ISerialisable;
                   item.Serialise();
                   select(item as T);
               }
               break;
           }
           case SerialiseMode.ResolvingRefs: {
               foreach (var item in collection) {
                   item.Serialise();
               }
               break;
           }
       }
       currentSaveNode = parent;
   }
    
    // Custom serialisation
    // User must handle raw JToken values themselves
    public void ArchiveCustom(string label, Func<JToken> save, Action<JToken> load, Action<JToken> resolveRefs) {
        var parent = currentSaveNode;
        switch (mode) {
            case SerialiseMode.Saving: {
                currentSaveNode[label] = save();
                break;
            }
            case SerialiseMode.Loading: {
                load(currentSaveNode[label]);
                break;
            }
            case SerialiseMode.ResolvingRefs: {
                resolveRefs(currentSaveNode[label]);
                break;
            }
        }
        currentSaveNode = parent;
    }
}