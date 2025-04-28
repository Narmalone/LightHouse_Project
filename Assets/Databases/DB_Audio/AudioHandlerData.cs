using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHandlerData
{
    public static AudioManager AudioManager { get; private set; }
    public static event Action OnAudioManagerInitialized;
    public static void Initialize(AudioManager audio)
    {
        AudioManager = audio;
        OnAudioManagerInitialized?.Invoke();
    }

    public static void Clear()
    {
        OnAudioManagerInitialized = null;
        AudioManager = null;
    }
}
