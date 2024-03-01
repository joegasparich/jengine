using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;
using Newtonsoft.Json.Linq;
using JEngine.entities;
using JEngine.util;
using JEngine.scenes;
using JEngine.ui;

namespace JEngine;

public class PlayerConfig {
    public int   screenWidth  = 1280;
    public int   screenHeight = 720;
    public float uiScale = 1f;
}

public class GameConfig {
    public double msPerUpdate  = 1000 / 60f;
    public int    worldScalePx = 1; // This is the scale of entity positions to screen pixels
}

public class Game {
    // Constants
    // TODO: Config options
    public const  int    DefaultScreenWidth  = 1280;
    public const  int    DefaultScreenHeight = 720;
    private const string ConfigFilePath      = "config.json";
    public const  int    LargerThanWorld     = 10000;

    public GameConfig   gameConfig;
    public PlayerConfig playerConfig;

    // Managers
    public InputManager   input;
    public Renderer       renderer;
    public AssetManager   assets;
    public SaveManager    saveManager;
    public SceneManager   sceneManager;
    public PhysicsManager physics;
    public UIManager      ui;

    // Collections
    private Dictionary<int, Entity>          entities;
    private List<Entity>                     entitiesToAdd;
    private List<Entity>                     entitiesToRemove;
    private Dictionary<string, List<Entity>> entitiesByTag = new();

    // State
    private int      ticksSinceGameStart;
    private int      framesSinceGameStart;
    private int      nextEntityId = 1;
    private bool     paused;

    // Properties
    public int      Ticks           => ticksSinceGameStart;
    public int      Frames          => framesSinceGameStart;
    public int      ScreenWidth     => Raylib.GetScreenWidth();
    public int      ScreenHeight    => Raylib.GetScreenHeight();
    public bool     IsPaused        => paused;

    public void Run() {
        Debug.Log("Application Started");
        Init();
        Debug.Log("Application Loaded");
        DoLoop();
        Debug.Log("Application Cleaning Up");
        Cleanup();
        Debug.Log("Application Ended");
    }

    public Game() {
        Find.Game = this;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(DefaultScreenWidth, DefaultScreenHeight, "JEngine");
        Raylib.SetExitKey(KeyboardKey.Null);

        gameConfig = new();
        playerConfig = new();

        input = new();
        renderer = new();
        assets = new();
        saveManager = new();
        sceneManager = new();
        physics = new();
        ui = new();

        entities = new();
        entitiesToAdd = new();
        entitiesToRemove = new();
    }

    protected virtual void Init() {
        renderer.Init();
        physics.Init();

        LoadConfig();

        Raylib.SetWindowSize(playerConfig.screenWidth, playerConfig.screenHeight);
        Raylib.InitAudioDevice();

        assets.LoadAssets();
        StaticConstructorUtility.CallConstructors();
    }

    protected virtual void Cleanup() {
        Raylib.CloseWindow();
    }

    private void DoLoop() {
        var    lastTime = Raylib.GetTime() * 1000;
        double lag      = 0;
        
        while (!Raylib.WindowShouldClose()) {
            var currentTime = Raylib.GetTime() * 1000;

            var elapsed = currentTime - lastTime;
            lag += elapsed;

            while (lag >= gameConfig.msPerUpdate) {
                if (!paused) {
                    // Do Update
                    PreUpdate();
                    Update();
                    PostUpdate();

                    ticksSinceGameStart++;
                }

                ConstantUpdate();

                lag -= gameConfig.msPerUpdate;
            }
            
            lastTime = currentTime;
            
            if (Raylib.IsWindowResized())
                OnScreenResized();
            
            // This needs to be out here otherwise we can get duplicate input events
            input.ProcessInput();
            
            renderer.Render();
        }
    }
    
    protected virtual void PreUpdate() {
        sceneManager.GetCurrentScene()?.PreUpdate();

        foreach (var entity in entities.Values) {
            try {
                entity.PreUpdate();
            } catch (Exception e) {
                Debug.Error($"Error in entity PreUpdate {entity.id}", e);
            }
        }
    }
    
    protected virtual void Update() {
        physics.Update();
        sceneManager.GetCurrentScene()?.Update();
        renderer.Update();

        foreach (var entity in entities.Values) {
            try {
                entity.Update();

                if (TickUtility.IsHashTick(entity.id, TickUtility.TickRareInterval))
                    entity.UpdateRare();

            } catch (Exception e) {
                Debug.Error($"Error in entity Update {entity.id}", e);
            }
        }
    }
    
    protected virtual void PostUpdate() {
        sceneManager.GetCurrentScene()?.PostUpdate();

        foreach (var entity in entities.Values) {
            try {
                entity.PostUpdate();
            } catch (Exception e) {
                Debug.Error($"Error in entity PostUpdate {entity.id}", e);
            }
        }
    }

    /// <summary>
    /// Runs every tick, regardless of pause state
    /// Occurs after PostUpdate
    /// </summary>
    protected virtual void ConstantUpdate() {
        sceneManager.GetCurrentScene()?.ConstantUpdate();

        DoEntityReg();
    }

    public virtual void Render() {
        sceneManager.GetCurrentScene()?.Render();

        foreach (var entity in entities.Values) {
            entity.Render();
        }

        framesSinceGameStart++;
    }

    public virtual void RenderLate() {
        sceneManager.GetCurrentScene()?.RenderLate();
        physics.RenderLate();
    }

    public virtual void Render2D() {
        ui.Render();
    }

    public virtual void OnInput(InputEvent evt) {
        foreach (var entity in entities.Values) {
            if (!evt.consumed)
                entity.OnInput(evt);
        }

        if (!evt.consumed)
            sceneManager.GetCurrentScene()?.OnInput(evt);

        if (!evt.consumed)
            renderer.camera.OnInput(evt);
    }

    public virtual void OnGUI() {
        Find.SceneManager.GetCurrentScene()?.OnGUI();

        foreach (var entity in entities.Values) {
            entity.OnGUI();
        }
    }

    protected virtual void OnScreenResized() {
        renderer.OnScreenResized();

        playerConfig.screenWidth = ScreenWidth;
        playerConfig.screenHeight = ScreenHeight;
        SaveConfig();
    }

        public int RegisterEntity(Entity entity, int? id) {
        entity.id = id ?? nextEntityId++;

        entitiesToAdd.Add(entity);

        Debug.Log($"Registered entity {entity.id}");

        return entity.id;
    }

    public int RegisterEntityNow(Entity entity, int id) {
        entity.id = id;

        entities.Add(entity.id, entity);
        entity.AddTag(EntityTags.All);
                
        foreach (var tag in entity.Tags) {
            Notify_EntityTagged(entity, tag);
        }

        return id;
    }
    
    public void Notify_EntityTagged(Entity entity, string tag) {
        if (!entitiesByTag.ContainsKey(tag))
            entitiesByTag.Add(tag, new List<Entity>());

        entitiesByTag[tag].Add(entity);
    }
    
    public void Notify_EntityUntagged(Entity entity, string tag) {
        if (!entitiesByTag.ContainsKey(tag))
            return;

        entitiesByTag[tag].Remove(entity);
    }

    public void UnregisterEntity(Entity entity) {
        entitiesToRemove.Add(entity);
    }

    private void DoEntityReg() {
        foreach (var entity in entitiesToAdd) {
            try {
                RegisterEntityNow(entity, entity.id);
                entity.Setup(false);
            } catch (Exception e) {
                Debug.Error($"Failed to set up entity {entity.id}:", e);
                entity.Destroy();
            }
        }
        entitiesToAdd.Clear();

        foreach (var entity in entitiesToRemove) {
            if (!entities.ContainsKey(entity.id))
                continue;

            entities.Remove(entity.id);
            
            foreach (var tag in entity.Tags) {
                entitiesByTag[tag].Remove(entity);
            }
        }
        entitiesToRemove.Clear();
    }

    public Entity? GetEntityById(int id) {
        if (!entities.ContainsKey(id))
            return null;

        return entities[id];
    }
    
    public IEnumerable<Entity> GetEntitiesByTag(string tag) {
        if (!entitiesByTag.ContainsKey(tag)) yield break;

        foreach (var entity in entitiesByTag[tag]) {
            yield return entity;
        }
    }

    public void ClearEntities() {
        foreach (var entity in entities) {
            entity.Value.Destroy();
        }
        foreach (var entity in entitiesToAdd) {
            entity.Destroy();
        }

        entities.Clear();
        entitiesToAdd.Clear();
        entitiesToRemove.Clear();
        entitiesByTag.Clear();
    }

    public void Pause(bool pause) {
        paused = pause;
    }
    public void TogglePause() {
        paused = !paused;
    }

    public virtual void Serialise() {
        saveManager.ArchiveValue("ticksSinceGameStart",  ref ticksSinceGameStart);
        saveManager.ArchiveValue("framesSinceGameStart", ref framesSinceGameStart);
        saveManager.ArchiveValue("nextEntityId",         ref nextEntityId);

        DoEntityReg();
        if (saveManager.mode == SerialiseMode.Loading)
            entities.Clear();

        saveManager.ArchiveDeep("scene", sceneManager.GetCurrentScene());
        saveManager.ArchiveCustom("entities",
            () => EntitySerialiseUtility.SaveEntities(entities.Values),
            data => EntitySerialiseUtility.LoadEntities(data as JArray),
            data => EntitySerialiseUtility.ResolveRefsEntities(data as JArray, entities.Values.ToList())
        );

        if (saveManager.mode == SerialiseMode.Loading)
            sceneManager.GetCurrentScene().PostLoad();
    }

    public void SaveConfig() {
        File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(playerConfig, Formatting.Indented));
    }

    public void LoadConfig() {
        if (!File.Exists(ConfigFilePath)) {
            SaveConfig();
            return;
        }
        
        playerConfig = JsonConvert.DeserializeObject<PlayerConfig>(File.ReadAllText(ConfigFilePath));
    }
}