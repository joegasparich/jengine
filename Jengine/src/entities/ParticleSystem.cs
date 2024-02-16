using System.Numerics;
using JEngine.util;
using Raylib_cs;

namespace JEngine.entities;

struct Particle {
    public bool    active;
    public Vector2 position;
    public float   scale;
    public Vector2 velocity;
    public int     lifetime;
    public Color   colour;
}

public class ParticleSettings {
    public Texture2D   texture;
    public int         count;
    public int         lifetime = 60;
    public Color       colour   = Color.White;
    public Gradient?   colourGradient;
    public Gradient?   colourOverLifetime;
    public FloatRange  speed = new(1, 1);
    public LerpPoints? speedOverLifetime;
    public FloatRange  direction = new (0, 360);
    public FloatRange  scale     = new (1, 1);
    public LerpPoints? scaleOverLifetime;
    public bool        fade = false;
}

public class ParticleSystem : Entity {
    private ParticleSettings settings;
    private Particle[]       particles;
    
    public ParticleSystem(ParticleSettings settings) {
        this.settings = settings;
        particles = new Particle[settings.count];
        
        for (var i = 0; i < particles.Length; i++) {
            particles[i] = new Particle {
                active = false,
                position = Vector2.Zero,
                lifetime = i % settings.lifetime,
            };
        }
    }
    
    public override void Update() {
        base.Update();
        
        for (var i = 0; i < particles.Length; i++) {
            particles[i].position += particles[i].velocity;
            particles[i].lifetime -= 1;
            var pct = 1 - particles[i].lifetime / (float)settings.lifetime;
            
            if (settings.speedOverLifetime != null)
                particles[i].velocity = particles[i].velocity.Normalised() * settings.speedOverLifetime.Calculate(pct);
            
            if (settings.scaleOverLifetime != null)
                particles[i].scale = settings.scaleOverLifetime.Calculate(pct);
            
            if (particles[i].lifetime <= 0) {
                var col = settings.colour;
                
                if (settings.colourOverLifetime != null)
                    col = settings.colourOverLifetime.Calculate(0);
                else if (settings.colourGradient != null)
                    col = settings.colourGradient.Calculate(Rand.Float(Find.Game.Ticks));

                var speed = settings.speedOverLifetime?.Calculate(0) ?? settings.speed.Random();
                var scale = settings.scaleOverLifetime?.Calculate(0) ?? settings.scale.Random();
                
                particles[i] = new Particle {
                    active   = true,
                    position = Vector2.Zero,
                    velocity = new Vector2(0, -speed).Rotate(settings.direction.Random() * JMath.DegToRad),
                    scale    = scale,
                    lifetime = settings.lifetime,
                    colour   = col
                };
            }
        }
    }

    public override void Render() {
        base.Render();
        
        for (var i = 0; i < particles.Length; i++) {
            if (!particles[i].active) 
                continue;

            var pct   = 1 - particles[i].lifetime / (float)settings.lifetime;
            var col   = particles[i].colour;
            if (settings.colourOverLifetime != null)
                col = settings.colourOverLifetime.Calculate(pct);
            
            if (settings.fade)
                col = col.WithAlpha(pct);
            
            Find.Renderer.Draw(
                texture: settings.texture,
                pos: pos * Find.Game.gameConfig.worldScalePx + particles[i].position,
                scale: new Vector2(particles[i].scale, particles[i].scale),
                depth: Find.Renderer.GetDepth(pos.Y),
                color: col
            );
        }
    }
}
