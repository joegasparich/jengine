using JEngine.scenes;
using JEngine.ui;

namespace JEngine;

public static class Find
{
    public static Game Game;

    public static GameConfig   Config       => Game.gameConfig;
    public static InputManager Input        => Game.input;
    public static Renderer     Renderer     => Game.renderer;
    public static AssetManager AssetManager => Game.assets;
    public static SceneManager SceneManager => Game.sceneManager;
    public static SaveManager  SaveManager  => Game.saveManager;
    public static Camera       Camera       => Renderer.camera;
    public static UIManager    UI           => Game.ui;
}