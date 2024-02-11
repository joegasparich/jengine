using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;
using Newtonsoft.Json.Linq;
using JEngine.entities;
using JEngine.util;
using JEngine.scenes;

namespace JEngine;

public class GameSettings {
    public int screenWidth  = 1280;
    public int screenHeight = 720;
}

public class Game {
    // Constants
    // TODO: Config options
    private const int    MsPerUpdate         = 10;
    private const int    DefaultScreenWidth  = 1280;
    private const int    DefaultScreenHeight = 720;
    private const string SettingsFilePath    = "settings.json";
    public const  int    LargerThanWorld     = 10000;

    public GameSettings settings;

    // Managers
    public InputManager input;
    public Renderer     renderer;
    public AssetManager assetManager;
    public SaveManager  saveManager;
    public SceneManager sceneManager;

    // Collections
    private Dictionary<int, Entity> entities;
    private List<Entity>            entitiesToAdd;
    private List<Entity>            entitiesToRemove;

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

        settings = new();

        input = new();
        renderer = new();
        assetManager = new();
        saveManager = new();
        sceneManager = new();

        entities = new();
        entitiesToAdd = new();
        entitiesToRemove = new();
    }

    protected virtual void Init() {
        renderer.Init();

        LoadSettings();

        Raylib.SetWindowSize(settings.screenWidth, settings.screenHeight);

        assetManager.LoadAssets();
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

            while (lag >= MsPerUpdate) {
                if (!paused) {
                    // Do Update
                    PreUpdate();
                    Update();
                    PostUpdate();

                    ticksSinceGameStart++;
                }

                ConstantUpdate();

                lag -= MsPerUpdate;
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
    }

    public virtual void Render2D() {
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

    protected virtual void OnScreenResized() {
        renderer.OnScreenResized();

        settings.screenWidth = ScreenWidth;
        settings.screenHeight = ScreenHeight;
        SaveSettings();
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

        return id;
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
        }
        entitiesToRemove.Clear();
    }

    public Entity? GetEntityById(int id) {
        if (!entities.ContainsKey(id))
            return null;

        return entities[id];
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

    public void SaveSettings() {
        File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
    }

    public void LoadSettings() {
        if (!File.Exists(SettingsFilePath)) {
            SaveSettings();
            return;
        }
        
        settings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(SettingsFilePath));
    }
}