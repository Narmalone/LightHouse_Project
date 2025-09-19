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
    const string VFX_Scale1 = "Scale1";
    const string VFX_Scale2 = "Scale2";

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

    void Update()
    {
        // ---- Mat & Lens
        WaterMaterial.SetFloat(_rippleStrengthID, RippleStrength);
        WaterMaterial.SetFloat(_waterMaskID, WaterMask);
        WaterMaterial.SetFloat(_smoothnessID, Smoothness);
        WaterLensMaterial.SetFloat(_isRaining, RainIntensity);

        // ---- Densité contrôlée (évite l’overdraw noir)
        // cible de particules "vivantes" selon l'intensité
        float targetAliveMin = 2e4f;    // 20k
        float targetAliveMax = 1.2e5f;  // 120k
        float targetAlive = Mathf.Lerp(targetAliveMin, targetAliveMax, RainIntensity);

        // Lifetime (si tu l’exposes dans le VFX sous le nom "Lifetime")
        float lifetime = Mathf.Lerp(4f, 1.2f, RainIntensity);
        if (RainVFX.HasFloat("Lifetime"))
            RainVFX.SetFloat("Lifetime", lifetime);

        // Spawn rate = alive / lifetime (cap sur ton max)
        float spawnRate = targetAlive / Mathf.Max(0.1f, lifetime);
        spawnRate = Mathf.Min(spawnRate, RainMaxIntensity);
        RainVFX.SetFloat(_rateId, spawnRate);
        // ---- Turbulence
        float turb = Mathf.Lerp(TurbulenceBase, TurbulenceAtMax, RainIntensity);
        RainVFX.SetFloat(VFX_NoiseIntensity, turb);

      /*  // ---- Échelle des gouttes (Vector3 !)
        Vector3 s1 = Vector3.Lerp(new Vector3(DropScale1Base.x, DropScale1Base.y, DropScale1Base.x),
                                  new Vector3(DropScale1Max.x, DropScale1Max.y, DropScale1Max.x), RainIntensity);
        Vector3 s2 = Vector3.Lerp(new Vector3(DropScale2Base.x, DropScale2Base.y, DropScale2Base.x),
                                  new Vector3(DropScale2Max.x, DropScale2Max.y, DropScale2Max.x), RainIntensity);
        RainVFX.SetVector3(VFX_Scale1, s1);
        RainVFX.SetVector3(VFX_Scale2, s2);*/

        // ---- Vitesse & vent
        Vector3 vBase = Vector3.Lerp(RainMinVelocity, RainMaxVelocity, RainIntensity);
        vBase *= Mathf.Lerp(1f, VelocityBoostAtMax, RainIntensity);
        Vector3 lateral = WindDirection.normalized * WindSpeed * (LateralFactorAtMax * RainIntensity);
        Vector3 v = vBase + lateral;
        RainVFX.SetVector3(_rainMinVelocityID, v);
        RainVFX.SetVector3(_rainMaxVelocityID, v);

        // ---- Audio
        if (mixer != null)
            mixer.SetFloat("RainVolume", Linear01ToDb(RainIntensity));
    }


    void LateUpdate()
    {
        /*// Rebranche ça dès que tu remets la météo en live
        if (WeatherHandlerData.CurrentWeather == null) return;

        // Humidity peut être 0..1 ou 0..100
        float h = WeatherHandlerData.CurrentWeather.Humidity;
        float humidityPercent = (h <= 1.0001f) ? h * 100f : h;

        RainIntensity = MapHumidityToRain01(humidityPercent);*/
    }
}
