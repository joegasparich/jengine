using JEngine;
using JEngine.entities;
using JEngine.scenes;
using SlasherExample.entities;

namespace SlasherExample.scenes;

public class Scene_Play : Scene
{
    private const string GuySpritePath = "characters/player.png";

    public Scene_Play(string name) : base(name) {}

    public override void Start() {
        base.Start();

        var graphic = new GraphicData();
        graphic.SetSpritesheet(GuySpritePath, 48, 48);

        var guy = Create.CreateEntity();
        guy.AddComponent<RenderComponent>(new RenderComponentData { GraphicData = graphic });
        guy.AddComponent<PhysicsComponent>();
        guy.AddComponent<MoveComponent>();
        guy.AddComponent<PlayerInputComponent>();
        guy.AddComponent<PlayerAnimationComponent>();
    }
}