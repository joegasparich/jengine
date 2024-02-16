using System.Numerics;
using JEngine.defs;
using JEngine.util;
using Newtonsoft.Json;
using Raylib_cs;

namespace JEngine;

public class AudioData {
    [JsonProperty] private string soundPath = "";

    public float volume    = 1;
    public float falloffPx = 2000;
    
    // Properties
    private Sound sound;
    public Sound Sound
    {
        get {
            if (!Raylib.IsSoundReady(sound))
                sound = Find.AssetManager.GetSound(soundPath);

            return sound;
        }
        set => sound = value;
    }
    
    [JsonConstructor]
    public AudioData() {}

    public AudioData(string path) {
        SetSound(path);
    }

    public void SetSound(string path) {
        soundPath = path;
        Sound     = Find.AssetManager.GetSound(soundPath);
    }

    public void PlayOnCamera() {
        Raylib.SetSoundVolume(Sound, volume);
        Raylib.PlaySound(Sound);
    }

    public void PlayAtPos(Vector2 worldPos) {
        var screenPos     = Find.Renderer.WorldToScreenPos(worldPos);
        var screenPosNorm = new Vector2(screenPos.X / Find.Game.ScreenWidth, screenPos.Y / Find.Game.ScreenHeight);
        Raylib.SetSoundVolume(Sound, JMath.Clamp01(volume * (1 - JMath.Clamp01(screenPosNorm.Distance(new Vector2(0.5f))))));
        Raylib.SetSoundPan(Sound, 1 - JMath.Clamp01(screenPosNorm.X));
        Raylib.PlaySound(Sound);
    }
}