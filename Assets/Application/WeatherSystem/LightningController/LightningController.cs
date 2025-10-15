using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;
using LightHouse.Weather;

/// <summary>
/// Orage "in-cloud" : ambiance + éclairs (EV additif négatif via LightingProfileManager) + tonnerre 3D.
/// Spawn: autour du joueur OU dans des zones. Durée flash & volume tonnerre dépendent de la distance.
/// Lit la hauteur nuages depuis VolumetricClouds (Unity 6 HDRP) si dispo.
/// </summary>
public class LightningController : MonoBehaviour
{
    public enum SpawnMode { AroundFollow, InZones }

    [Header("Storm")]
    [Range(0f, 1f)] public float StormLevel = 0f;                 // 0..1 fréquence/intensité globale
    public Vector2 LightningIntervalRange = new Vector2(6f, 20f); // à StormLevel=1
    public Vector2 IntervalJitter = new Vector2(0.7f, 1.3f);      // petite variation

    [Header("Spawn - Around Follow")]
    public float SkyRadiusMin = 800f;
    public float SkyRadiusMax = 2500f;
    public float SkyHeight = 1200f;

    [Header("Spawn - Zones (recommandé)")]
    public BoxCollider[] SkyZones;
    public Vector2 ZoneHeightOffset = new Vector2(600f, 1600f);

    [Header("Refs")]
    public SpawnMode spawnMode = SpawnMode.InZones;
    public Transform Follow; // souvent la caméra/joueur

    // ------------------ LIGHTING (EV OVERRIDE) ------------------
    [Header("Lighting (exposure override)")]
    public LightHouse.Game.Rendering.LightingProfileManager lightingManager;

    [Tooltip("Courbe de forme du flash (0..1 -> 0..1).")]
    public AnimationCurve flashCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.05f, 1f), new Keyframe(0.25f, 0f)
    );

    [Header("Flash duration by distance")]
    [Tooltip("Durée de base à ~1 km (s).")]
    public float baseFlashDuration = 0.55f;
    [Tooltip("Allongement (s) par km. Durée = base + km * facteur (clampée).")]
    public float flashDurPerKm = 0.12f;
    public Vector2 flashDurClamp = new Vector2(0.15f, 1.4f);

    [Header("Flash strength by distance (EV négatif)")]
    [Tooltip("Pic EV ABS (sera envoyé en NÉGATIF).")]
    public float flashEVMaxAbs = 3.5f;
    [Tooltip("Distance (km) au-delà de laquelle l'EV → 0 (normalisation).")]
    public float evMaxDistanceKm = 12f;
    [Tooltip("Courbe de modulateur EV vs distance normalisée [0..1] (0 proche, 1 = evMaxDistanceKm). 1=fort, 0=faible).")]
    public AnimationCurve evByDistance01 = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(1f, 0f)
    );
    [Tooltip("Multiplicateur global appliqué après la courbe (pour peaufiner).")]
    public float evStrengthMul = 1f;

    [Header("Flash Light (optionnel)")]
    public bool addSkyDirectional = false;
    public Light flashLight;
    public float lightIntensityMax = 50000f; // lumen HDRP / intensity URP

    // ------------------ AUDIO ------------------
    [Header("Audio Mixer / Ambiance")]
    public AudioMixer mixer; // expose p.ex. "Orage_Ambient"
    public AudioSource ambientLoop; // grondement continu (2D)
    public AnimationCurve ambientVolFromStorm = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0f, 0.5f)] public float ambientSmooth = 0.2f;
    float _ambV, _ambVel;

    [Header("Thunder SFX (one-shots 3D)")]
    public AudioCue thunderClips;
    [Range(0.6f, 1.1f)] public float thunderPitchMin = 0.9f;
    [Range(0.6f, 1.1f)] public float thunderPitchMax = 1.05f;
    public Vector2 thunderVolumeDbRange = new Vector2(-18f, 0f); // min..max dB

    [Header("Thunder volume by distance")]
    [Tooltip("Distance (km) max considérée pour normalisation de la courbe.")]
    public float thunderMaxDistanceKm = 12f;
    [Tooltip("Courbe 0..1 en fonction de la distance normalisée (0 proche → 1 = maxDist). Valeur → multiplicateur linéaire.")]
    public AnimationCurve thunderVolByDistance01 = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(1f, 0f)
    );
    [Tooltip("Gain global appliqué après la courbe (linéaire). 1 = neutre.")]
    public float thunderVolumeMul = 1f;

    // ------------------ CLOUDS (HDRP Unity 6) ------------------
    [Header("Cloud layer (auto from Volume)")]
    public Volume globalVolume; // Volume avec VolumetricClouds

    // ------------------ PHYSIQUE / SCHED ------------------
    [Header("Physique")]
    public float speedOfSound = 343f;

    [Header("Scheduling")]
    public bool autoRun = true;
    Coroutine _runner;

    private bool IsStorming = false;

    // ---------- UNITY ----------

    private void Awake()
    {
        WeatherHandlerData.OnWeatherTypeChanged += OnWeatherTypeChanged;
    }

    private void OnDestroy()
    {
        WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherTypeChanged;
    }

    void Start()
    {
        if (!Follow) Follow = Camera.main ? Camera.main.transform : transform;


        if (autoRun && _runner == null) _runner = StartCoroutine(RunStorm());
        if (ambientLoop && !ambientLoop.isPlaying) ambientLoop.Play();

        if (WeatherHandlerData.CurrentWeather != null)
            CheckStorm(WeatherHandlerData.CurrentWeather.WeatherType);
    }

    void Update()
    {
        float targetAmb01 = Mathf.Clamp01(ambientVolFromStorm.Evaluate(StormLevel));
        _ambV = Mathf.SmoothDamp(_ambV, targetAmb01, ref _ambVel, ambientSmooth);
        if (mixer) mixer.SetFloat("Orage_Ambient", Linear01ToDb(_ambV));
    }

    private void LateUpdate()
    {
        TryFetchCloudLayerFromVolume(globalVolume);
    }

    private void OnWeatherTypeChanged(WeatherType type)
    {
        CheckStorm(type);
    }

    private void CheckStorm(WeatherType type)
    {
        if (type == WeatherType.Stormy)
        {
            StartStorm();
            IsStorming = true;
            StormLevel = 1f;
        }
        else if (_runner != null)
        {
            StopStorm();
            IsStorming = false;
            StormLevel = 0f;
        }
    }

    // ---------- LOOP ----------
    IEnumerator RunStorm()
    {
        var wait = new WaitForEndOfFrame();

        while (true)
        {
            float inten = Mathf.Clamp01(StormLevel);
            float min = Mathf.Lerp(12f, LightningIntervalRange.x, inten);
            float max = Mathf.Lerp(35f, LightningIntervalRange.y, inten);
            float interval = Random.Range(min, max) * Random.Range(IntervalJitter.x, IntervalJitter.y);

            float t = 0f;
            while (t < interval) { t += Time.deltaTime; yield return wait; }

            SpawnLightningEvent();
        }
    }

    // ---------- EVENT ----------
    void SpawnLightningEvent()
    {
        Vector3 center = Follow ? Follow.position : transform.position;
        Vector3 pos = GetRandomSkyPosition(center);

        float distMeters = Vector3.Distance(center, pos);
        float km = distMeters / 1000f;
        float delay = distMeters / Mathf.Max(1f, speedOfSound);

        // Flash EV (distance-aware)
        StartCoroutine(DoFlashEV(km));

        // Tonnerre
        StartCoroutine(PlayThunderAfterDelay(pos, delay, km));
    }

    // ---------- SPAWN POS ----------
    Vector3 GetRandomSkyPosition(Vector3 followCenter)
    {
        if (spawnMode == SpawnMode.InZones && SkyZones != null && SkyZones.Length > 0)
        {
            var zone = SkyZones[Random.Range(0, SkyZones.Length)];
            var b = zone.bounds;
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);
            float y = b.max.y + Random.Range(ZoneHeightOffset.x, ZoneHeightOffset.y);
            return new Vector3(x, y, z);
        }
        else
        {
            float radius = Random.Range(SkyRadiusMin, SkyRadiusMax);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pos = followCenter + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            pos.y = SkyHeight;
            return pos;
        }
    }

    // ---------- FLASH EV ----------
    IEnumerator DoFlashEV(float distKm)
    {
        if (!lightingManager) yield break;

        // Durée dépend de la distance
        float dur = Mathf.Clamp(baseFlashDuration + distKm * flashDurPerKm, flashDurClamp.x, flashDurClamp.y);

        // EV négatif modulé par distance
        float n = Mathf.InverseLerp(0f, Mathf.Max(0.001f, evMaxDistanceKm), distKm); // 0 proche → 1 loin
        float evAtt01 = Mathf.Clamp01(evByDistance01.Evaluate(n)) * Mathf.Max(0f, evStrengthMul);
        float peakAbs = flashEVMaxAbs * evAtt01;
        float peakSigned = -Mathf.Abs(peakAbs); // négatif = plus clair au sol (selon ton pipeline)

        // Lampe optionnelle
        if (addSkyDirectional && flashLight)
        {
            flashLight.transform.position = (Follow ? Follow.position : transform.position) + Vector3.up * 1000f;
            flashLight.transform.forward = Vector3.down;
            flashLight.enabled = true;
        }

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float x = Mathf.Clamp01(t / dur);
            float k = flashCurve.Evaluate(x);       // 0..1
            float ev = Mathf.Lerp(0f, peakSigned, k);
            lightingManager.SetAdditionalExposure(ev);

            if (addSkyDirectional && flashLight)
                flashLight.intensity = Mathf.Lerp(0f, lightIntensityMax, k);

            yield return null;
        }

        lightingManager.SetAdditionalExposure(0f);
        if (addSkyDirectional && flashLight) { flashLight.intensity = 0f; flashLight.enabled = false; }
    }

    // ---------- THUNDER ----------
    IEnumerator PlayThunderAfterDelay(Vector3 pos, float delay, float distKm)
    {
        yield return new WaitForSeconds(delay);

        if (thunderClips == null || ServiceLocator.Audio == null) yield break;

        // 3D attenuation de l'AudioSource existe déjà, mais on ajoute NOTRE courbe distance
        float n = Mathf.InverseLerp(0f, Mathf.Max(0.001f, thunderMaxDistanceKm), distKm); // 0→1
        float distMul01 = Mathf.Clamp01(thunderVolByDistance01.Evaluate(n));              // 1 proche → 0 loin
        float stormMul01 = Mathf.Clamp01(StormLevel);                                     // plus d'orage → plus fort

        // Map dB min..max, puis conversion → linéaire, puis * distMul * stormMul * mul global
        float db = Mathf.Lerp(thunderVolumeDbRange.x, thunderVolumeDbRange.y, stormMul01);
        float lin = DbToLinear01(db) * distMul01 * Mathf.Max(0f, thunderVolumeMul);

        IAudioHandle audioHandle = ServiceLocator.Audio.PlayAt(thunderClips, pos);
        audioHandle.SetPitch(Random.Range(thunderPitchMin, thunderPitchMax));
        audioHandle.SetVolume(lin);
    }

    // ---------- CLOUDS: fetch depuis Volume (Unity 6 HDRP) ----------
    public void TryFetchCloudLayerFromVolume(Volume vol)
    {
        if (vol.profile.TryGet(out VolumetricClouds clouds) && clouds != null)
        {
            SkyHeight = clouds.altitudeRange.value;
        }
    }

    static float Linear01ToDb(float v01) => (v01 <= 0.0001f) ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(v01));
    static float DbToLinear01(float db) => Mathf.Pow(10f, db / 20f);

    // API publique
    public void SetStormLevel(float v) => StormLevel = Mathf.Clamp01(v);
    public void StartStorm() { if (_runner == null) _runner = StartCoroutine(RunStorm()); }
    public void StopStorm() { if (_runner != null) StopCoroutine(_runner); _runner = null; }
}