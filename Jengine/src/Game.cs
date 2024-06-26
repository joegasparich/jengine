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
    public int   ScreenWidth  = 1280;
    public int   ScreenHeight = 720;
    public float UiScale = 1f;
}

public class GameConfig {
    public double MsPerUpdate  = 1000 / 60f;
    public int    WorldScalePx = 1; // This is the scale of entity positions to screen pixels
}

public class Game {
    // Constants
    // TODO: Config options
    public const  int    DefaultScreenWidth  = 1280;
    public const  int    DefaultScreenHeight = 720;
    private const string ConfigFilePath      = "config.json";

    public GameConfig   GameConfig;
    public PlayerConfig PlayerConfig;

    // Managers
    public InputManager   Input;
    public Renderer       Renderer;
    public AssetManager   Assets;
    public SaveManager    SaveManager;
    public SceneManager   SceneManager;
    public PhysicsManager Physics;
    public UIManager      UI;

    // Collections
    private Dictionary<int, Entity?>          entities;
    private List<Entity?>                     entitiesToAdd;
    private List<Entity?>                     entitiesToRemove;
    private Dictionary<string, List<Entity?>> entitiesByTag = new();

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

        GameConfig = new();
        PlayerConfig = new();

        Input = new();
        Renderer = new();
        Assets = new();
        SaveManager = new();
        SceneManager = new();
        Physics = new();
        UI = new();

        entities = new();
        entitiesToAdd = new();
        entitiesToRemove = new();
    }

    protected virtual void Init() {
        Renderer.Init();
        Physics.Init();
        UI.Init();

        LoadConfig();

        Raylib.SetWindowSize(PlayerConfig.ScreenWidth, PlayerConfig.ScreenHeight);
        Raylib.InitAudioDevice();

        Assets.LoadAssets();
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

            while (lag >= GameConfig.MsPerUpdate) {
                if (!paused) {
                    // Do Update
                    PreUpdate();
                    Update();
                    PostUpdate();

                    ticksSinceGameStart++;
                }

                ConstantUpdate();

                lag -= GameConfig.MsPerUpdate;
            }
            
            lastTime = currentTime;
            
            if (Raylib.IsWindowResized())
                OnScreenResized();
            
            // This needs to be out here otherwise we can get duplicate input events
            Input.ProcessInput();
            
            UI.PreDraw();
            Renderer.Draw();
            UI.PostDraw();
         
            // Free any managed RL assets
            Assets.DoUnload();
        }
    }
    
    protected virtual void PreUpdate() {
        SceneManager.GetCurrentScene()?.PreUpdate();

        foreach (var entity in entities.Values) {
            try {
                entity.PreUpdate();
            } catch (Exception e) {
                Debug.Error($"Error in entity PreUpdate {entity.Id}", e);
            }
        }
    }
    
    protected virtual void Update() {
        Physics.Update();
        SceneManager.GetCurrentScene()?.Update();
        Renderer.Update();

        foreach (var entity in entities.Values) {
            try {
                entity.Update();

                if (TickUtility.IsHashTick(entity.Id, TickUtility.TickRareInterval))
                    entity.UpdateRare();

            } catch (Exception e) {
                Debug.Error($"Error in entity Update {entity.Id}", e);
            }
        }
    }
    
    protected virtual void PostUpdate() {
        SceneManager.GetCurrentScene()?.PostUpdate();

        foreach (var entity in entities.Values) {
            try {
                entity.PostUpdate();
            } catch (Exception e) {
                Debug.Error($"Error in entity PostUpdate {entity.Id}", e);
            }
        }
    }

    /// <summary>
    /// Runs every tick, regardless of pause state
    /// Occurs after PostUpdate
    /// </summary>
    protected virtual void ConstantUpdate() {
        SceneManager.GetCurrentScene()?.ConstantUpdate();

        DoEntityReg();
    }

    public virtual void Draw() {
        SceneManager.GetCurrentScene()?.Draw();

        foreach (var entity in entities.Values) {
            entity.Draw();
        }
    }

    public virtual void DrawLate() {
        SceneManager.GetCurrentScene()?.DrawLate();

        foreach (var entity in entities.Values) {
            entity.DrawLate();
        }

        Physics.DrawLate();

        framesSinceGameStart++;
    }

    public virtual void DrawUI() {
        UI.DrawUI();
    }

    public virtual void OnInput(InputEvent evt) {
        UI.OnInput(evt);
        
        foreach (var entity in entities.Values) {
            entity.OnInput(evt);
        }

        SceneManager.GetCurrentScene()?.OnInput(evt);
        Physics.OnInput(evt);
        Renderer.Camera.OnInput(evt);
        
        UI.PostInput(evt);
    }

    public virtual void OnGUI() {
        Find.SceneManager.GetCurrentScene()?.OnGUI();

        foreach (var entity in entities.Values) {
            entity.OnGUI();
        }
    }

    protected virtual void OnScreenResized() {
        Renderer.OnScreenResized();
        UI.OnScreenResized();

        PlayerConfig.ScreenWidth = ScreenWidth;
        PlayerConfig.ScreenHeight = ScreenHeight;
        SaveConfig();
    }

    public int RegisterEntity(Entity? entity, int? id = null) {
        entity.Id = id ?? nextEntityId++;

        entitiesToAdd.Add(entity);

        return entity.Id;
    }

    public int RegisterEntityNow(Entity? entity, int id) {
        entity.Id = id;

        entities.Add(entity.Id, entity);
        entity.AddTag(EntityTags.All);
                
        foreach (var tag in entity.Tags) {
            Notify_EntityTagged(entity, tag);
        }

        if (!entity.IsSetup) {
            Debug.Error($"Entity {entity.Name} is being registered without being set up");
            entity.Setup(false);
        }

        return id;
    }
    
    public void Notify_EntityTagged(Entity? entity, string tag) {
        if (!entitiesByTag.ContainsKey(tag))
            entitiesByTag.Add(tag, new List<Entity?>());

        entitiesByTag[tag].Add(entity);
    }
    
    public void Notify_EntityUntagged(Entity? entity, string tag) {
        if (!entitiesByTag.ContainsKey(tag))
            return;

        entitiesByTag[tag].Remove(entity);
    }

    public void UnregisterEntity(Entity? entity) {
        entitiesToRemove.Add(entity);
    }

    private void DoEntityReg() {
        foreach (var entity in entitiesToAdd) {
            try {
                RegisterEntityNow(entity, entity.Id);
            } catch (Exception e) {
                Debug.Error($"Failed to set up entity {entity.Id}:", e);
                entity.Destroy();
            }
        }
        entitiesToAdd.Clear();

        foreach (var entity in entitiesToRemove) {
            if (!entities.ContainsKey(entity.Id))
                continue;

            entities.Remove(entity.Id);
            
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
    
    public IEnumerable<Entity?> GetEntitiesByTag(string tag) {
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
        SaveManager.ArchiveValue("ticksSinceGameStart",  ref ticksSinceGameStart);
        SaveManager.ArchiveValue("framesSinceGameStart", ref framesSinceGameStart);
        SaveManager.ArchiveValue("nextEntityId",         ref nextEntityId);

        DoEntityReg();
        if (SaveManager.Mode == SerialiseMode.Loading)
            entities.Clear();

        SaveManager.ArchiveDeep("scene", SceneManager.GetCurrentScene());
        SaveManager.ArchiveCustom("entities",
            () => EntitySerialiseUtility.SaveEntities(entities.Values),
            data => EntitySerialiseUtility.LoadEntities(data as JArray),
            data => EntitySerialiseUtility.ResolveRefsEntities(data as JArray, entities.Values.ToList())
        );

        if (SaveManager.Mode == SerialiseMode.Loading)
            SceneManager.GetCurrentScene().PostLoad();
    }

    public void SaveConfig() {
        File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(PlayerConfig, Formatting.Indented));
    }

    public void LoadConfig() {
        if (!File.Exists(ConfigFilePath)) {
            SaveConfig();
            return;
        }
        
        PlayerConfig = JsonConvert.DeserializeObject<PlayerConfig>(File.ReadAllText(ConfigFilePath));
    }
}