using LightHouse.Weather;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class RainController : MonoBehaviour
{
    [Header("Materials and VFX")]
    public Material WaterMaterial;
    public Material WaterLensMaterial;
    public VisualEffect RainVFX;
    [SerializeField] private CustomPassVolume _rainVolume;

    [Header("Ripple Settings")]
    [Range(0, 1)] public float RippleStrength = 0.3f;
    [Range(0, 1)] public float WaterMask = 1f;
    [Range(0, 1)] public float Smoothness = 0f;

    [Header("Rain Settings")]
    public float RainMaxIntensity = 100000f;   // spawn max pour le VFX
    [Range(0, 1)] public float RainIntensity = 0f; // 0..1 (piloté par météo)

    // vitesses verticales de base (sans vent)
    public Vector3 RainMinVelocity = new Vector3(0, -24, 0);
    public Vector3 RainMaxVelocity = new Vector3(0, -30, 0);

    [Header("Humidity → Rain mapping (%RH)")]
    [Range(0, 100)] public float HumidityRainStart = 70f; // pas de pluie en dessous
    [Range(0, 100)] public float HumidityRainFull = 95f; // pluie max à partir d'ici
    [Tooltip("Rend la montée très raide près du max (1=linéaire, 2..4 plus raide)")]
    [Range(1f, 4f)] public float DelugeGamma = 2.5f;
    [Tooltip("Au-dessus du seuil de déluge, boost multiplicatif d’intensité")]
    public float DelugeBoost = 3f;
    [Range(0, 100)] public float DelugeHumidity = 98f;  // à partir d’ici, on booste

    [Header("Storm tuning")]
    [Tooltip("Multiplicateur de vitesse verticale à pleine pluie")]
    public float VelocityBoostAtMax = 1.5f;       // 1.0 = inchangé, 1.5 = 50% plus vite
    [Tooltip("Turbulence du VFX à 0 pluie → pleine pluie")]
    public float TurbulenceBase = 0.5f;
    public float TurbulenceAtMax = 4f;
    [Tooltip("Taille des sprites (Scale1/2 dans ton VFX) à 0 → max")]
    public Vector2 DropScale1Base = new Vector2(0.12f, 1.0f);
    public Vector2 DropScale1Max = new Vector2(0.20f, 1.2f);
    public Vector2 DropScale2Base = new Vector2(0.02f, 0.2f);
    public Vector2 DropScale2Max = new Vector2(0.04f, 0.25f);

    [Header("Wind (inclinaison des gouttes)")]
    public Vector3 WindDirection = new Vector3(1, 0, 0);
    public float WindSpeed = 0f;                  // m/s “ressenti” jeu
    public float LateralFactorAtMax = 1.0f;       // combien le vent penche la pluie à pleine intensité

    [Header("Audio")]
    public AudioMixer mixer;                      // exposer "RainVolume" dans le mixer

    // shader props
    static readonly int _rippleStrengthID = Shader.PropertyToID("_RippleStrength");
    static readonly int _waterMaskID = Shader.PropertyToID("_Mask");
    static readonly int _smoothnessID = Shader.PropertyToID("_Smoothness");
    static readonly int _rateId = Shader.PropertyToID("Rate");
    static readonly int _rainMaxVelocityID = Shader.PropertyToID("Velocity2");
    static readonly int _rainMinVelocityID = Shader.PropertyToID("Velocity1");
    static readonly int _isRaining = Shader.PropertyToID("_isRaining");

    // VFX extra props (d’après ton graph)
    const string VFX_NoiseIntensity = "NoiseIntensity";

    // helpers
    static float Linear01ToDb(float v01) => (v01 <= 0.0001f) ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(v01));
    static float Smooth01(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

    float MapHumidityToRain01(float humidityPercent)
    {
        // map %RH -> [0..1] puis courbe plus raide proche du max
        float t = Mathf.InverseLerp(HumidityRainStart, HumidityRainFull, humidityPercent);
        t = Smooth01(t);
        t = Mathf.Pow(t, 1f / Mathf.Max(1f, DelugeGamma)); // accentue la pente vers 1
        // boost “déluge” quand on dépasse le seuil
        float boost = humidityPercent >= DelugeHumidity ? DelugeBoost : 1f;
        return Mathf.Clamp01(t * boost);
    }

    private void Start()
    {
        RainIntensity = 0f;
    }

    // en haut de la classe
    const float INTENSITY_ON = 0.03f;   // seuil d’activation (3%)
    const float INTENSITY_OFF = 0.015f;  // seuil d’extinction (1.5%) => hystérésis
    bool _vfxRunning;                    // état courant

    void Update()
    {
        // ---- Mat & Lens
        WaterMaterial.SetFloat(_rippleStrengthID, RippleStrength);
        WaterMaterial.SetFloat(_waterMaskID, WaterMask);
        WaterMaterial.SetFloat(_smoothnessID, Smoothness);
        WaterLensMaterial.SetFloat(_isRaining, RainIntensity);

        // ---- Gestion seuils / hystérésis
        bool wantRunning = _vfxRunning
            ? (RainIntensity > INTENSITY_OFF)
            : (RainIntensity >= INTENSITY_ON);

        if (wantRunning && !_vfxRunning)
        {
            RainVFX.Reinit();   // purge buffers & compteurs; évite un “burst” résiduel
            RainVFX.Play();
            _vfxRunning = true;
            if (_rainVolume) _rainVolume.enabled = true;
        }
        else if (!wantRunning && _vfxRunning)
        {
            // on arrête : plus aucun spawn, laisse mourir les particules, puis Stop()
            RainVFX.SetFloat(_rateId, 0f);
            RainVFX.Stop();
            _vfxRunning = false;
            if (_rainVolume) _rainVolume.enabled = false;
        }

        // ---- Densité contrôlée
        // à 0 → 0, à 1 → targetAliveMax
        float targetAliveMax = 1.2e5f;
        float targetAlive = Mathf.Lerp(0f, targetAliveMax, RainIntensity);

        // Lifetime (si exposé)
        float lifetime = Mathf.Lerp(4f, 1.2f, RainIntensity);
        if (RainVFX.HasFloat("Lifetime"))
            RainVFX.SetFloat("Lifetime", lifetime);

        // Spawn rate = alive / lifetime (capé)
        float spawnRate = _vfxRunning ? targetAlive / Mathf.Max(0.1f, lifetime) : 0f;
        spawnRate = Mathf.Min(spawnRate, RainMaxIntensity);
        RainVFX.SetFloat(_rateId, spawnRate);

        // ---- Turbulence
        float turb = Mathf.Lerp(TurbulenceBase, TurbulenceAtMax, RainIntensity);
        RainVFX.SetFloat(VFX_NoiseIntensity, _vfxRunning ? turb : 0f);

        // ---- Vitesse & vent
        Vector3 vBase = Vector3.Lerp(RainMinVelocity, RainMaxVelocity, RainIntensity);
        vBase *= Mathf.Lerp(1f, VelocityBoostAtMax, RainIntensity);
        Vector3 lateral = WindDirection.normalized * WindSpeed * (LateralFactorAtMax * RainIntensity);
        Vector3 v = _vfxRunning ? (vBase + lateral) : Vector3.zero;
        RainVFX.SetVector3(_rainMinVelocityID, v);
        RainVFX.SetVector3(_rainMaxVelocityID, v);

        // ---- Audio
        if (mixer != null)
        {
            float vol01 = _vfxRunning ? RainIntensity : 0f;
            mixer.SetFloat("RainVolume", Linear01ToDb(vol01));
        }
    }

    void LateUpdate()
    {
        if (WeatherHandlerData.CurrentWeather == null) return;

        float h = WeatherHandlerData.CurrentWeather.Humidity;
        float humidityPercent = (h <= 1.0001f) ? h * 100f : h;

        // Si tu aimes ton “quand il pleut / quand il pleut pas”, on garde la même map,
        // mais tu peux lisser davantage la base en relevant HumidityRainStart de 1–2 pts.
        RainIntensity = MapHumidityToRain01(humidityPercent);
    }

}
