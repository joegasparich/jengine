using System.Numerics;
using JEngine.defs;
using JEngine.util;
using Newtonsoft.Json;
using Raylib_cs;

namespace JEngine;

public class Audio {
    [JsonProperty] private string _soundPath = "";

    public float Volume    = 1;
    public float FalloffPx = 2000;
    
    // Properties
    private Sound _sound;
    public Sound Sound
    {
        get {
            if (!Raylib.IsSoundReady(_sound))
                _sound = Find.AssetManager.GetSound(_soundPath);

            return _sound;
        }
        set => _sound = value;
    }
    
    [JsonConstructor]
    public Audio() {}

    public Audio(string path) {
        SetSound(path);
    }

    public void SetSound(string path) {
        _soundPath = path;
        Sound     = Find.AssetManager.GetSound(_soundPath);
    }

    public void PlayOnCamera() {
        Raylib.SetSoundVolume(Sound, Volume);
        Raylib.PlaySound(Sound);
    }

    public void PlayAtPos(Vector2 worldPos) {
        var screenPos     = Find.Renderer.WorldToScreenPos(worldPos);
        var screenPosNorm = new Vector2(screenPos.X / Find.Game.ScreenWidth, screenPos.Y / Find.Game.ScreenHeight);
        Raylib.SetSoundVolume(Sound, JMath.Clamp01(Volume * (1 - JMath.Clamp01(screenPosNorm.Distance(new Vector2(0.5f))))));
        Raylib.SetSoundPan(Sound, 1 - JMath.Clamp01(screenPosNorm.X));
        Raylib.PlaySound(Sound);
    }
}