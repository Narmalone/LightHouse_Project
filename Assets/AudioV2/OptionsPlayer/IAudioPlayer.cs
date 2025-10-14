using UnityEngine;
using UnityEngine.Audio;

public interface IAudioPlayer
{
    IAudioHandle PlayAt(string cueId, Vector3 position, AudioPlayOptions opt = default);
    IAudioHandle PlayAt(AudioCue cue, Vector3 position, AudioPlayOptions opt = default);

    IAudioHandle PlayOneShot(string cueId, AudioPlayOptions opt = default); // non-spatial UI/2D
    IAudioHandle PlayOneShot(AudioCue cue, AudioPlayOptions opt = default);

    void Preload(string cueId); // addressables/streaming si besoin
    void StopAllOn(GameObject owner);
}

public interface IAudioHandle
{
    AudioClip SelectedClip { get; }
    bool IsValid { get; }
    void Stop(float fadeOut = 0f);
    void SetVolume(float v);
    void SetPitch(float p);
    void SetPosition(Vector3 p); // pour la source 3D suivie
}

public struct AudioPlayOptions
{
    public GameObject Owner;
    public float VolumeMul;
    public float PitchMul;
    public bool Persistent;
    public AudioMixerGroup OverrideGroup;

    public bool? Force2D;           // null = utilise le cue ; true/false = override
    public float? SpatialBlend;     // si tu veux forcer un blend précis (0..1)
    public bool FollowTransform;
}

