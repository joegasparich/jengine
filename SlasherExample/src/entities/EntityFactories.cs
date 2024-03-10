using System.Numerics;
using JEngine;
using JEngine.entities;

namespace SlasherExample.entities;

public static class EntityTags {
    public const string Player = "Player";
}

public static class EntityFactories {
    private const string GuySpritePath = "characters/player.png";
    private const string SlimeSpritePath = "characters/slime.png";
    
    public static Entity CreatePlayer(Vector2 pos) {
        var graphic = new Graphic();
        graphic.SetSpritesheet(GuySpritePath, 48, 48);
        graphic.origin = new Vector2(0.5f);
        
        var player = Create.CreateEntity();
        player.Transform.LocalPosition = pos;
        player.AddComponent<RenderComponent>(new RenderComponentData { Graphic = graphic });
        player.AddComponent<PhysicsComponent>();
        player.AddComponent<MoveComponent>();
        player.AddComponent<PlayerInputComponent>();
        player.AddComponent<PersonAnimationComponent>();
        player.AddComponent<CameraFollowComponent>();
        player.AddComponent<AttackerComponent>();
        
        player.AddTag(EntityTags.Player);
        player.SetName("Player");

        return player;
    }
    
    public static Entity CreateSlime(Vector2 pos) {
        var graphic = new Graphic();
        graphic.SetSpritesheet(SlimeSpritePath, 32, 32);
        graphic.origin = new Vector2(0.5f);
        
        var slime = Create.CreateEntity();
        slime.Transform.LocalPosition = pos;
        slime.AddComponent<RenderComponent>(new RenderComponentData { Graphic = graphic });
        slime.AddComponent<PhysicsComponent>();
        var move = slime.AddComponent<MoveComponent>();
        move.acceleration = 0.0015f;
        slime.AddComponent<EnemyAIComponent>();
        var anim = slime.AddComponent<EnemyAnimationComponent>();
        anim.AddAnimation(EnemyAnimationComponent.Idle, new Animation(0, 4, 48));
        anim.AddAnimation(EnemyAnimationComponent.Walk, new Animation(7, 6, 48));

        slime.SetName("Slime");

        return slime;
    }
}