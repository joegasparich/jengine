using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Raylib_cs;
using JEngine.defs;
using JEngine.util;

namespace JEngine;

public class AssetManager {
    // Constants
    private const string DefPath = "assets/defs";
    private const string TexturePath = "assets/textures";
    private const string SoundPath = "assets/sounds";
    private readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings {
        Converters = new List<JsonConverter> {
            new DefConverter(),
            new TypeConverter(),
            new LerpPointsConverter()
        },
    });
    private Texture2D defaultTexture;
    private Sound defaultSound;
    
    // State
    private readonly Dictionary<string, Texture2D>             textureMap     = new();
    private readonly Dictionary<string, Sound>                 soundMap       = new();
    private readonly Dictionary<Type, Dictionary<string, Def>> defMap         = new();
    private readonly Dictionary<string, Def>                   defMapFlat     = new();
    public readonly  List<(object, ExpressionValueProvider)>   unresolvedDefs = new();

    public void LoadAssets() {
        LoadTextures();
        LoadSounds();
        
        // Defs
        var dataQueue = GetDataQueue();
        var resolvedQueue = ResolveJsonInheritance(dataQueue);
        DeserializeDefs(resolvedQueue);

        ResolveDefReferences();

        DefUtility.LoadDefOfs();
    }

    private void LoadTextures() {
        foreach (var path in FileUtility.GetFiles(TexturePath, "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".png"))
                continue;

            try {
                GetTexture(path);
            } catch (Exception e) {
                Debug.Error($"Failed to load texture with path: {path}", e);
                textureMap.Remove(path);
            }
        }
        
        defaultTexture = GetTexture(TexturePath + "/placeholder.png");
    }

    private void LoadSounds() {
        foreach (var path in FileUtility.GetFiles(SoundPath, "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".wav"))
                continue;

            try {
                GetSound(path);
            } catch (Exception e) {
                Debug.Error($"Failed to load sound with path: {path}", e);
                soundMap.Remove(path);
            }
        }
        
        defaultSound = GetSound(SoundPath + "/placeholder.wav");
    }

    private static Queue<JObject> GetDataQueue() {
        var dataQueue  = new Queue<JObject>();
        
        foreach (var path in FileUtility.GetFiles(DefPath, "*.*", SearchOption.AllDirectories)) {
            if (!path.EndsWith(".json"))
                continue;

            try {
                var json = File.ReadAllText(path);
                try {
                    // Try get as list of defs
                    var dataList = JsonConvert.DeserializeObject<List<JObject>>(json);
                    foreach (var data in dataList) {
                        var id = data["id"]?.ToString();
                        if (id == null) {
                            Debug.Error($"Failed to load def, missing id: {data}");
                            continue;
                        }

                        dataQueue.Enqueue(data);
                    }
                } catch {
                    // Fallback to single def
                    var data = JsonConvert.DeserializeObject<JObject>(json)!;
                    dataQueue.Enqueue(data);
                }
            } catch (Exception e) {
                Debug.Error($"Failed to load json with path: {path}", e);
            }
        }

        return dataQueue;
    }

    private static Queue<JObject> ResolveJsonInheritance(Queue<JObject> dataQueue) {
        var resolvedQueue = new Queue<JObject>();
        var resolvedDict  = new Dictionary<string, JObject>();
        
        while (dataQueue.Count > 0) {
            var data = dataQueue.Dequeue();
            if (!data.HasValues) {
                Debug.Warn("Empty def found, ignoring");
                continue;
            }

            var id = data["id"]!.Value<string>();
            if (id == null) {
                Debug.Warn("Found def missing id, ignoring");
                continue;
            }
            
            if (!data.ContainsKey("abstract"))
                data.Add("abstract", false);

            try {
                if (data.TryGetValue("inherits", out var inherits)) {
                    var inheritsId = inherits.Value<string>();
                    if (resolvedDict.ContainsKey(inheritsId)) {
                        // Merge with child's data as priority
                        var merged = (JObject)resolvedDict[inheritsId].DeepClone();
                        merged.Merge(data);
                        data = merged;
                    } else {
                        if (dataQueue.All(d => d["id"]?.Value<string>() != inheritsId)) {
                            Debug.Error($"Failed to resolve inheritance for {id}, missing def: {inheritsId}");
                            continue;
                        }

                        dataQueue.Enqueue(data);
                        continue;
                    }
                }
                
                resolvedQueue.Enqueue(data);
                resolvedDict.Add(id, data);
            } catch (Exception e) {
                Debug.Error($"Failed to resolve inheritance for {id}", e);
            }
        }

        return resolvedQueue;
    }

    private void DeserializeDefs(Queue<JObject> dataQueue) {
        while (dataQueue.Count > 0) {
            var data = dataQueue.Dequeue();
            var id = data["id"].ToString();
            if (data["abstract"].Value<bool>())
                continue;

            // Check if already loaded
            if (defMapFlat.ContainsKey(id)) {
                Debug.Error($"Failed to load def {id}, id already exists");
                continue;
            }

            // Get type
            var typeString = data["class"]?.ToString();
            if (typeString == null) {
                Debug.Error($"Failed to load def {id}, no type specified");
                continue;
            }

            var type = TypeUtility.GetTypeByName(typeString);
            if (type == null) {
                Debug.Error($"Failed to load def {id}, type {typeString} doesn't exist");
                continue;
            }

            Debug.Log($"Loading {typeString} with id {id}");

            // Instantiate
            Def instance;
            try {
                instance = (Def)data.ToObject(type, serializer)!;
            } catch (Exception e) {
                Debug.Error($"Failed to load def {id}, could not deserialize json", e);
                continue;
            }

            // Register
            if (!defMap.ContainsKey(type))
                defMap.Add(type, new Dictionary<string, Def>());

            defMap[type].Add(id, instance);
            defMapFlat[id] = instance;
        }
    }

    private void ResolveDefReferences() {
        foreach (var unresolvedDef in unresolvedDefs) {
            var (obj, provider) = unresolvedDef;
            var val = provider.GetValue(obj);

            if (val is Def def)
                provider.SetValue(obj, GetDef(def.id));
            else if (val.GetType().IsGenericType && val.GetType().GetGenericTypeDefinition() == typeof(List<>)) {
                var list        = (IList)val;
                var genericList = Activator.CreateInstance(val.GetType()) as IList;
                foreach (Def d in list) {
                    genericList.Add(GetDef(d.id));
                }

                provider.SetValue(obj, genericList);
            } else
                Debug.Error("Failed to resolve def reference, unsupported collection: " + val.GetType().Name);

            // TODO: Resolve defs in other collections
        }
    }

    public Texture2D GetTexture(string path) {
        var shortPath = path.Replace(TexturePath + "/", "");

        if (!textureMap.ContainsKey(shortPath)) {
            Texture2D texture;

            path = ValidatePath(path);
            
            if (!File.Exists(path)) {
                Debug.Warn("Could not find path to texture " + path);
                texture = defaultTexture;
            } else {
                texture = Raylib.LoadTexture(path);
            }
            
            textureMap.Add(shortPath, texture);
        }

        return textureMap[shortPath];
    }
    
    public Sound GetSound(string path) {
        var shortPath = path.Replace(SoundPath + "/", "");

        if (!soundMap.ContainsKey(shortPath)) {
            Sound sound;

            path = ValidatePath(path);
            
            if (!File.Exists(path)) {
                Debug.Warn("Could not find path to sound " + path);
                sound = defaultSound;
            } else {
                sound = Raylib.LoadSound(path);
            }
            
            soundMap.Add(shortPath, sound);
        }

        return soundMap[shortPath];
    }

    public JObject GetJson(string path) {
        path = ValidatePath(path);

        if (!File.Exists(path)) {
            Debug.Warn("Could not find JSON file at " + path);
            return null;
        }

        try {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<JObject>(json)!;
            return data;
        } catch (Exception e) {
            Debug.Error($"Failed to load json with path: {path}", e);
        }

        return null;
    }

    public Def? GetDef(string id, bool suppressError = false) {
        if (!defMapFlat.ContainsKey(id)) {
            if (!suppressError)
                Debug.Error($"Failed to get def with id {id}, no defs of that type have been loaded");
            return null;
        }
        
        return defMapFlat[id];
    }

    public Def? GetDef(Type type, string id) {
        if (!defMap.ContainsKey(type)) {
            // Try get it from the flat map if it's a subclass
            if (defMapFlat.ContainsKey(id)) {
                var def = defMapFlat[id];
                if (def.DefType.IsSubclassOf(type))
                    return def;
            }
            
            Debug.Error($"Failed to get def of type {type}, no defs of that type have been loaded");
            return null;
        }

        if (!defMap[type].ContainsKey(id)) {
            Debug.Error($"Failed to get def of type {type} with id {id}, no def with that id has been loaded");
            return null;
        }

        return defMap[type][id];
    }

    public T? GetDef<T>(string id) where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            // Try get it from the flat map if it's a subclass
            if (defMapFlat.ContainsKey(id)) {
                var def = defMapFlat[id];
                if (def.DefType.IsSubclassOf(typeof(T)))
                    return def as T;
            }
            
            Debug.Error($"Failed to get def of type {typeof(T)}, no defs of that type have been loaded");
            return null;
        }

        if (!defMap[typeof(T)].ContainsKey(id)) {
            Debug.Error($"Failed to get def of type {typeof(T)} with id {id}, no def with that id has been loaded");
            return null;
        }

        return (T)defMap[typeof(T)][id];
    }
    
    public List<T>? GetAllDefs<T>() where T : Def {
        if (!defMap.ContainsKey(typeof(T))) {
            Debug.Error($"Failed to get defs of type {typeof(T)}, no defs of that type have been loaded");
            return null;
        }
        
        // TODO: Cache this

        return defMap[typeof(T)].Values.Cast<T>().ToList();
    }

    private string ValidatePath(string path) {
        if (!path.Contains("assets/"))
            path = path.Insert(0, "assets/");

        return path;
    }
}