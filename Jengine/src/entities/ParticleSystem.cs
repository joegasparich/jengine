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
    private ParticleSettings _settings;
    private Particle[]       _particles;
    
    public ParticleSystem(ParticleSettings settings) {
        _settings = settings;
        _particles = new Particle[settings.Count];
        
        for (var i = 0; i < _particles.Length; i++) {
            _particles[i] = new Particle {
                Active = false,
                Position = Vector2.Zero,
                Lifetime = i % settings.Lifetime,
            };
        }
    }
    
    public override void Update() {
        base.Update();
        
        for (var i = 0; i < _particles.Length; i++) {
            _particles[i].Position += _particles[i].Velocity;
            _particles[i].Lifetime -= 1;
            var pct = 1 - _particles[i].Lifetime / (float)_settings.Lifetime;
            
            if (_settings.SpeedOverLifetime != null)
                _particles[i].Velocity = _particles[i].Velocity.Normalised() * _settings.SpeedOverLifetime.Calculate(pct);
            
            if (_settings.ScaleOverLifetime != null)
                _particles[i].Scale = _settings.ScaleOverLifetime.Calculate(pct);
            
            if (_particles[i].Lifetime <= 0)
                _particles[i] = CreateParticle();
        }
    }

    private Particle CreateParticle() {
        var col = _settings.Colour;
                
        if (_settings.ColourOverLifetime != null)
            col = _settings.ColourOverLifetime.Calculate(0);
        else if (_settings.ColourGradient != null)
            col = _settings.ColourGradient.Calculate(Rand.Float(Find.Game.Ticks));

        var speed = _settings.SpeedOverLifetime?.Calculate(0) ?? _settings.Speed.Random();
        var scale = _settings.ScaleOverLifetime?.Calculate(0) ?? _settings.Scale.Random();
        
        return new Particle {
            Active   = true,
            Position = Vector2.Zero,
            Velocity = new Vector2(0, -speed).Rotate(_settings.Direction.Random() * JMath.DegToRad),
            Scale    = scale,
            Lifetime = _settings.Lifetime,
            Colour   = col
        };
    }

    public override void Draw() {
        base.Draw();
        
        for (var i = 0; i < _particles.Length; i++) {
            if (!_particles[i].Active) 
                continue;

            var pct   = 1 - _particles[i].Lifetime / (float)_settings.Lifetime;
            var col   = _particles[i].Colour;
            if (_settings.ColourOverLifetime != null)
                col = _settings.ColourOverLifetime.Calculate(pct);
            
            if (_settings.Fade)
                col = col.WithAlpha(pct);
            
            Find.Renderer.Draw(
                texture: _settings.Texture,
                pos: Transform.GlobalPosition * Find.Game.GameConfig.WorldScalePx + _particles[i].Position,
                scale: new Vector2(_particles[i].Scale, _particles[i].Scale),
                depth: Find.Renderer.GetDepth(Transform.GlobalPosition.Y),
                color: col
            );
        }
    }
}
