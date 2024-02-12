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
    private const string GuySpritePath = "characters/player.png";
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

        var graphic = new GraphicData();
        graphic.SetSpritesheet(GuySpritePath, 48, 48);
        graphic.origin = new Vector2(0.5f);

        player = Create.CreateEntity();
        player.pos = new Vector2(8, 8);
        player.AddComponent<RenderComponent>(new RenderComponentData { GraphicData = graphic });
        player.AddComponent<PhysicsComponent>();
        player.AddComponent<MoveComponent>();
        player.AddComponent<PlayerInputComponent>();
        player.AddComponent<PlayerAnimationComponent>();
        player.AddComponent<CameraFollowComponent>();

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

    public override void Render() {
        base.Render();

        tileManager.Render();
    }
}