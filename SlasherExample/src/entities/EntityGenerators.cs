using System.Numerics;
using JEngine;
using JEngine.entities;

namespace SlasherExample.entities;

public static class EntityTags {
    public const string Player = "Player";
}

public static class EntityGenerators {
    private const string GuySpritePath = "characters/player.png";
    private const string SlimeSpritePath = "characters/slime.png";
    
    public static Entity CreatePlayer(Vector2 pos) {
        var graphic = new GraphicData();
        graphic.SetSpritesheet(GuySpritePath, 48, 48);
        graphic.origin = new Vector2(0.5f);
        
        var player = Create.CreateEntity();
        player.pos = pos;
        player.AddComponent<RenderComponent>(new RenderComponentData { GraphicData = graphic });
        player.AddComponent<PhysicsComponent>();
        player.AddComponent<MoveComponent>();
        player.AddComponent<PlayerInputComponent>();
        player.AddComponent<PersonAnimationComponent>();
        player.AddComponent<CameraFollowComponent>();
        
        player.AddTag(EntityTags.Player);

        return player;
    }
    
    public static Entity CreateSlime(Vector2 pos) {
        var graphic = new GraphicData();
        graphic.SetSpritesheet(SlimeSpritePath, 32, 32);
        graphic.origin = new Vector2(0.5f);
        
        var slime = Create.CreateEntity();
        slime.pos = pos;
        slime.AddComponent<RenderComponent>(new RenderComponentData { GraphicData = graphic });
        slime.AddComponent<PhysicsComponent>();
        var move = slime.AddComponent<MoveComponent>();
        move.acceleration = 0.0015f;
        slime.AddComponent<EnemyAIComponent>();
        var anim = slime.AddComponent<EnemyAnimationComponent>();
        anim.AddAnimation(EnemyAnimationComponent.Idle, new Animation(0, 4, 48));
        anim.AddAnimation(EnemyAnimationComponent.Walk, new Animation(7, 6, 48));

        return slime;
    }
}