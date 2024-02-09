using JEngine.scenes;

namespace JEngine;

public static class Find
{
    public static Game Game;

    public static InputManager Input        => Game.input;
    public static Renderer     Renderer     => Game.renderer;
    public static AssetManager AssetManager => Game.assetManager;
    public static SceneManager SceneManager => Game.sceneManager;
    public static SaveManager  SaveManager  => Game.saveManager;
}