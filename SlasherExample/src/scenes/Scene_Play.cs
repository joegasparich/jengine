using System.Numerics;
using JEngine;
using JEngine.entities;
using JEngine.scenes;
using Raylib_cs;
using SlasherExample.entities;

namespace SlasherExample.scenes;

public class Scene_Play : Scene
{
    // Constants
    private const string LevelPath     = "levels/level.ldtk";

    // State
    private TileManager tileManager = new();
    private Entity      player;

    public Scene_Play(string name) : base(name) {}

    public override void Start() {
        base.Start();

        Find.Game.physics.RegisterCollider(new Rectangle(-1, -1, 1, 18));
        Find.Game.physics.RegisterCollider(new Rectangle(-1, -1, 18, 1));
        Find.Game.physics.RegisterCollider(new Rectangle(16, -1, 1, 18));
        Find.Game.physics.RegisterCollider(new Rectangle(-1, 16, 18, 1));

        player = EntityGenerators.CreatePlayer(new Vector2(8, 8));

        EntityGenerators.CreateSlime(new Vector2(14, 14));

        tileManager.LoadLDTKLevel(LevelPath, 0);
    }

    public override void OnInput(InputEvent evt) {
        if (evt.consumed)
            return;

        if (evt.mouseDown == MouseButton.Left) {
            var worldPos = evt.mouseWorldPos;
            player.pos = worldPos;
        }
    }

    // public override void PostUpdate() {
    //     if (Find.Renderer.GetPickIdAtPos(Find.Input.GetMousePos()) == player.id) {
    //         player.Renderer.OverrideColour = Color.SkyBlue;
    //     }
    // }

    public override void Render() {
        base.Render();

        tileManager.Render();
    }
}