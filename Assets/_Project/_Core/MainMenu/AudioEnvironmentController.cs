using LightHouse.Core.Player;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioEnvironmentController : PersistentSingleton<AudioEnvironmentController>
{
    public AudioMixer mixer;

    public static event Action<EnvironmentState> OnEnvironmentStateChanged;

    [Header("Occlusion")]
    public string occlusionLowpassParam = "Occlusion_LowPass";
    public float indoorLowpass = 5000f;
    public float outdoorLowpass = 20000f;

    [Header("Global Exposure (ALL SYSTEMS)")]
    [Range(0f, 1f)] public float globalExposure = 0f; // runtime (lissé)
    public float exposureSpeed = 2f;

    private float _targetExposure;

    // MULTI CONTRIBUTORS (fenêtres, pluie, vagues, etc)
    private float _totalExposure = 0f;
    private int _contributors = 0;

    public EnvironmentState CurrentEnvironmentState;
    public bool IsStateOccluded => CurrentEnvironmentState == EnvironmentState.Indoor ? true : false;

    // ──────────────────────────────────────────────────────────────────────

    void Update()
    {
        UpdateEnvironmentState();

        // Lissage global
        globalExposure = Mathf.Lerp(
            globalExposure,
            _targetExposure,
            1f - Mathf.Exp(-exposureSpeed * Time.deltaTime)
        );

        ApplyOcclusion();
    }

    void LateUpdate()
    {
        float avgExposure = (_contributors > 0)
            ? _totalExposure / _contributors
            : 0f;

        SetExposure(avgExposure);

        // RESET FRAME
        _totalExposure = 0f;
        _contributors = 0;
    }

    // ──────────────────────────────────────────────────────────────────────

    private void UpdateEnvironmentState()
    {
        if (PlayerHandlerData.MainPlayer == null) return;

        if (PlayerHandlerData.IsPlayerOccluded())
            SetIndoor();
        else
            SetOutdoor();
    }

    public void SetIndoor()
    {
        CurrentEnvironmentState = EnvironmentState.Indoor;
        OnEnvironmentStateChanged?.Invoke(CurrentEnvironmentState);
    }

    public void SetOutdoor()
    {
        CurrentEnvironmentState = EnvironmentState.Outdoor;
        OnEnvironmentStateChanged?.Invoke(CurrentEnvironmentState);
    }

    public void SetExposure(float value)
    {
        _targetExposure = Mathf.Clamp01(value);
    }

    // ──────────────────────────────────────────────────────────────────────

    private void ApplyOcclusion()
    {
        if (!mixer) return;

        float baseLowpass = (CurrentEnvironmentState == EnvironmentState.Indoor)
            ? indoorLowpass
            : outdoorLowpass;

        // 🔥 Blend global exposure
        float finalLowpass = Mathf.Lerp(baseLowpass, outdoorLowpass, globalExposure);

        mixer.SetFloat(occlusionLowpassParam, finalLowpass);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 🔥 API GÉNÉRIQUE (TOUS les systèmes)
    public void RegisterExposure(float exposure)
    {
        _totalExposure += Mathf.Clamp01(exposure);
        _contributors++;
    }
}