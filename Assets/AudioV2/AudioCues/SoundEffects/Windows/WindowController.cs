using LightHouse.Handlers;
using LightHouse.Weather;
using UnityEngine;
using UnityEngine.Audio;

public class WindowController : MonoBehaviour
{
    [Header("Audio")]
    public AudioCue creakingWoodCue;       // loop discret continu
    public AudioCue creakScreamWoodCue;    // one-shot gros craquement
    public AudioMixer mixer;               // pour LPF/EQ/Pitch de groupe
    public string LowpassParam = "Window_Lowpass_Hz";
    public string EqBodyParam = "Window_EQ_Body_dB";
    public string EqCreakParam = "Window_EQ_Creak_dB";
    public string PitchParam = "Window_Pitch";

    [Header("Vent -> Volume (loop)")]
    public float windAtMin = 0f;           // m/s -> volume 0 sur la courbe (x=0)
    public float windAtMax = 130f;         // m/s -> volume 1 sur la courbe (x=1)
    public AnimationCurve windToVolumeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float volumeSmoothing = 10f;    // lissage (0 = instantané)

    [Header("Pitch (léger, optionnel)")]
    public float pitchMin = 0.98f;
    public float pitchMax = 1.06f;

    [Header("Gros craquement (one-shot)")]
    public bool enableScreamCreak = true;
    public float windAtExtreme = 200f;     // SEUIL de déclenchement des screams
    public float screamEveryMin = 4f;      // timer aléatoire simple
    public float screamEveryMax = 10f;
    [Range(0f, 2f)] public float screamVolume = 1.0f; // volume fixe du one-shot
    [Range(0f, 0.3f)] public float screamPitchJitterSemitones = 0.08f;

    private IAudioHandle _loopHandle;
    private float _volRuntime = 0f;
    private float _screamTimer = -1f;      // <0 = timer désarmé

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (ServiceLocator.Audio != null && creakingWoodCue != null)
        {
            _loopHandle = ServiceLocator.Audio.PlayAt(creakingWoodCue, transform.position);
            _loopHandle?.SetVolume(0f);
        }

        // par défaut : mode indoor (LPF/EQ), on ne touche pas au volume mixer
        SetIndoorMode();

        // init immédiate
        float wind = CurrentWind();
        ApplyWindToLoop(wind, immediate: true);
        _screamTimer = -1f; // désarmé au start
    }

    void Update()
    {
        float wind = CurrentWind();

        // bascule indoor/outdoor (LPF/EQ uniquement)
        if (PlayerHandlerData.MainPlayer != null)
        {
            if (PlayerHandlerData.IsPlayerOccluded()) SetIndoorMode();
            else SetOutdoorMode();
        }

        // Loop: volume via AnimationCurve + pitch léger
        ApplyWindToLoop(wind, immediate: false);

        // Scream: timer simple quand au-dessus du seuil
        UpdateScream(wind);
    }
    // ──────────────────────────────────────────────────────────────────────

    private float CurrentWind()
    {
        return (WeatherHandlerData.CurrentWeather != null)
            ? WeatherHandlerData.CurrentWeather.WindSpeed
            : 0f;
    }

    // ── MODES (EQ/LPF uniquement) ────────────────────────────────────────
    public void SetIndoorMode()
    {
        if (!mixer) return;
        mixer.SetFloat(LowpassParam, 8000f);
        mixer.SetFloat(EqBodyParam, +3f);
        mixer.SetFloat(EqCreakParam, +2f);
    }

    public void SetOutdoorMode()
    {
        if (!mixer) return;
        mixer.SetFloat(LowpassParam, 20000f);
        mixer.SetFloat(EqBodyParam, 0f);
        mixer.SetFloat(EqCreakParam, 0f);
    }

    // ── Loop: volume (AnimationCurve) + pitch léger ──────────────────────
    private void ApplyWindToLoop(float wind, bool immediate)
    {
        if (_loopHandle == null) return;

        // normalisation 0..1 sur [windAtMin, windAtMax]
        float t = Mathf.Clamp01(Mathf.InverseLerp(windAtMin, windAtMax, wind));

        // volume via courbe (x=t, y=volume)
        float targetVol = Mathf.Clamp01(windToVolumeCurve.Evaluate(t));

        // lissage exponentiel
        if (immediate || volumeSmoothing <= 0f)
            _volRuntime = targetVol;
        else
            _volRuntime = Mathf.Lerp(_volRuntime, targetVol, 1f - Mathf.Exp(-volumeSmoothing * Time.deltaTime));

        _loopHandle.SetVolume(_volRuntime);

        // pitch léger
        float pitch = Mathf.Lerp(pitchMin, pitchMax, t);
        _loopHandle.SetPitch(pitch);
        if (mixer) mixer.SetFloat(PitchParam, pitch);
    }

    // ── Scream: simple timer aléatoire quand vent >= windAtExtreme ───────
    private void UpdateScream(float wind)
    {
        if (!enableScreamCreak || creakScreamWoodCue == null || ServiceLocator.Audio == null)
            return;

        if (wind >= windAtExtreme)
        {
            // armer le timer si désarmé
            if (_screamTimer < 0f)
                _screamTimer = Random.Range(screamEveryMin, screamEveryMax);

            // décrémenter et tirer
            _screamTimer -= Time.deltaTime;
            if (_screamTimer <= 0f)
            {
                FireScream();
                _screamTimer = Random.Range(screamEveryMin, screamEveryMax); // re-arme
            }
        }
        else
        {
            // en-dessous du seuil: on désarme
            _screamTimer = -1f;
        }
    }

    private void FireScream()
    {
        var h = ServiceLocator.Audio.PlayAt(creakScreamWoodCue, transform.position);
        if (h == null) return;

        h.SetVolume(screamVolume);

        if (screamPitchJitterSemitones > 0f)
        {
            float semi = Random.Range(-screamPitchJitterSemitones, screamPitchJitterSemitones);
            float factor = Mathf.Pow(2f, semi / 12f);
            h.SetPitch(factor);
        }
    }

    // Helpers Inspector
    [ContextMenu("Mode: Indoor")] private void CtxIndoor() => SetIndoorMode();
    [ContextMenu("Mode: Outdoor")] private void CtxOutdoor() => SetOutdoorMode();
}
