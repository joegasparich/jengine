using JEngine.scenes;
using JEngine.ui;

namespace JEngine;

public static class Find
{
    public static Game Game;

    public static GameConfig     Config       => Game.GameConfig;
    public static InputManager   Input        => Game.Input;
    public static Renderer       Renderer     => Game.Renderer;
    public static AssetManager   AssetManager => Game.Assets;
    public static SceneManager   SceneManager => Game.SceneManager;
    public static SaveManager    SaveManager  => Game.SaveManager;
    public static Camera         Camera       => Renderer.Camera;
    public static UiManager      Ui           => Game.Ui;
    public static PhysicsManager Physics      => Game.Physics;
}