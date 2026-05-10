using LightHouse.Core.Audio;
using UnityEngine;

[System.Serializable]
public class BootStep
{
    [TextArea]
    public string text;

    [Header("Timing")]
    public float baseCharDelay = 0.03f;
    public float randomCharDelay = 0.05f;
    public float postDelay = 0.5f;

    [Header("Visual")]
    [ColorUsage(true, true)] public Color textColor = Color.white;
    public bool glitch;

    [Header("Typing Glitch")]
    public bool randomPauses;
    public float pauseChance = 0.1f;
    public float pauseDuration = 0.2f;

    [Header("Audio")]
    public SO_AudioCue typingSound;
    public SO_AudioCue glitchSound;
}