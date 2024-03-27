using System.Numerics;
using JEngine.util;
using Raylib_cs;

namespace JEngine.entities;

struct Particle {
    public bool    Active;
    public Vector2 Position;
    public float   Scale;
    public Vector2 Velocity;
    public int     Lifetime;
    public Color   Colour;
}

public class ParticleSettings {
    public Texture2D   Texture;
    public int         Count;
    public int         Lifetime = 60;
    public Color       Colour   = Color.White;
    public Gradient?   ColourGradient;
    public Gradient?   ColourOverLifetime;
    public FloatRange  Speed = new(1, 1);
    public LerpPoints? SpeedOverLifetime;
    public FloatRange  Direction = new (0, 360);
    public FloatRange  Scale     = new (1, 1);
    public LerpPoints? ScaleOverLifetime;
    public bool        Fade = false;
}

public class ParticleSystem : Entity {
    private ParticleSettings settings;
    private Particle[]       particles;
    
    public ParticleSystem(ParticleSettings settings) {
        this.settings = settings;
        particles = new Particle[settings.Count];
        
        for (var i = 0; i < particles.Length; i++) {
            particles[i] = new Particle {
                Active = false,
                Position = Vector2.Zero,
                Lifetime = i % settings.Lifetime,
            };
        }
    }
    
    public override void Update() {
        base.Update();
        
        for (var i = 0; i < particles.Length; i++) {
            particles[i].Position += particles[i].Velocity;
            particles[i].Lifetime -= 1;
            var pct = 1 - particles[i].Lifetime / (float)settings.Lifetime;
            
            if (settings.SpeedOverLifetime != null)
                particles[i].Velocity = particles[i].Velocity.Normalised() * settings.SpeedOverLifetime.Calculate(pct);
            
            if (settings.ScaleOverLifetime != null)
                particles[i].Scale = settings.ScaleOverLifetime.Calculate(pct);
            
            if (particles[i].Lifetime <= 0)
                particles[i] = CreateParticle();
        }
    }

    private Particle CreateParticle() {
        var col = settings.Colour;
                
        if (settings.ColourOverLifetime != null)
            col = settings.ColourOverLifetime.Calculate(0);
        else if (settings.ColourGradient != null)
            col = settings.ColourGradient.Calculate(Rand.Float(Find.Game.Ticks));

        var speed = settings.SpeedOverLifetime?.Calculate(0) ?? settings.Speed.Random();
        var scale = settings.ScaleOverLifetime?.Calculate(0) ?? settings.Scale.Random();
        
        return new Particle {
            Active   = true,
            Position = Vector2.Zero,
            Velocity = new Vector2(0, -speed).Rotate(settings.Direction.Random()),
            Scale    = scale,
            Lifetime = settings.Lifetime,
            Colour   = col
        };
    }

    public override void Draw() {
        base.Draw();
        
        for (var i = 0; i < particles.Length; i++) {
            if (!particles[i].Active) 
                continue;

            var pct   = 1 - particles[i].Lifetime / (float)settings.Lifetime;
            var col   = particles[i].Colour;
            if (settings.ColourOverLifetime != null)
                col = settings.ColourOverLifetime.Calculate(pct);
            
            if (settings.Fade)
                col = col.WithAlpha(pct);
            
            Find.Renderer.Draw(
                texture: settings.Texture,
                pos: Transform.GlobalPosition * Find.Game.GameConfig.WorldScalePx + particles[i].Position,
                scale: new Vector2(particles[i].Scale, particles[i].Scale),
                depth: Find.Renderer.GetDepth(Transform.GlobalPosition.Y),
                color: col
            );
        }
    }
}
