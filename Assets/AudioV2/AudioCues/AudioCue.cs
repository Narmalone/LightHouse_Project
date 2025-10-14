using UnityEngine;
using UnityEngine.Audio;

public enum AudioCategory { Music, Ambience, SFX, UI, Voice }
public enum Spatialization { TwoD, ThreeD }

[CreateAssetMenu(menuName = "Audio/AudioCue")]
public class AudioCue : ScriptableObject
{
    public string Id; // clé stable (ex: "UI/Click", "Env/Rain/Heavy")
    public AudioCategory Category;
    public AudioMixerGroup MixerGroup;
    public CueVariant[] Variants;  // multi-clips + settings
    public bool Loop;
    public AudioRolloffMode Rolloff = AudioRolloffMode.Logarithmic;
    public float MinDistance = 1f;
    public float MaxDistance = 50f;
    public float Volume = 1f;      // niveau par défaut
    public float Pitch = 1f;
    public float RandomPitch = 0.0f; // ± variation
    public float RandomVolume = 0.0f;
    public int MaxSimultaneousVoices = 0; // 0 = illimité (bus-level limiter prendra relais)

    public Spatialization Spatial = Spatialization.ThreeD; // par défaut 3D
    public float StereoPan = 0f; // utile pour UI
    public bool BypassListenerFx = false;
    public bool BypassReverbZones = false;
    public float DopplerLevel = 1f; // pour 2D mets 0
}

[System.Serializable]
public class CueVariant
{
    public AudioClip Clip;
    [Range(0, 1)] public float Weight = 1f; // random weighted
}

