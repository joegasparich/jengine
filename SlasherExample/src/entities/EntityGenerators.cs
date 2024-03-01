using System.Numerics;
using JEngine;
using JEngine.entities;

namespace SlasherExample.entities;

public static class EntityTags {
    public const string Player = "Player";
}

public static class EntityGenerators {
    private const string GuySpritePath = "characters/player.png";
    
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
    
    public static Entity CreateEnemy(Vector2 pos) {
        var graphic = new GraphicData();
        graphic.SetSpritesheet(GuySpritePath, 48, 48);
        graphic.origin = new Vector2(0.5f);
        
        var enemy = Create.CreateEntity();
        enemy.pos = pos;
        enemy.AddComponent<RenderComponent>(new RenderComponentData { GraphicData = graphic });
        enemy.AddComponent<PhysicsComponent>();
        var move = enemy.AddComponent<MoveComponent>();
        move.acceleration = 0.0015f;
        enemy.AddComponent<EnemyAIComponent>();
        enemy.AddComponent<PersonAnimationComponent>();

        return enemy;
    }
}