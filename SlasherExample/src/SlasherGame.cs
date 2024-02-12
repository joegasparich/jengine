using JEngine;
using Raylib_cs;
using SlasherExample.scenes;

namespace SlasherExample;

public class SlasherGame : Game
{
    private const string SaveFileName = "slasher";

    protected override void Init() {
        gameConfig.worldScalePx = 16;

        base.Init();

        Find.Camera.zoom = 4f;

        Find.SceneManager.LoadScene(new Scene_Play("Play"));
    }

    public override void OnInput(InputEvent evt) {
        if (Find.Input.IsKeyHeld(KeyboardKey.LeftControl) && evt.keyDown == KeyboardKey.S) {
            Find.SaveManager.SaveCurrentScene(SaveFileName, true);
        }

        if (Find.Input.IsKeyHeld(KeyboardKey.LeftControl) && evt.keyDown == KeyboardKey.L) {
            Find.SceneManager.LoadScene(new Scene_Play("Play"));
            Find.SaveManager.LoadIntoCurrentScene(SaveFileName);
        }

        base.OnInput(evt);
    }
}