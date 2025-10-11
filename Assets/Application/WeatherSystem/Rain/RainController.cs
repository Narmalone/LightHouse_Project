using LightHouse.Weather;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;
using UnityEngine.Rendering.HighDefinition;

[ExecuteInEditMode]
public class RainController : MonoBehaviour
{
    // -------- References --------
    [Header("Materials & VFX")]
    public Material WaterMaterial;
    public Material WaterLensMaterial;
    public VisualEffect RainVFX;
    [SerializeField] private CustomPassVolume _rainVolume;

    [Header("Follow (VFX position)")]
    [Tooltip("La cible dont on veut caler l'altitude (souvent la caméra ou le joueur).")]
    public Transform YFollowTarget;
    [Tooltip("Si activé, on verrouille la position Y du GameObject au Y de la cible + offset.")]
    public bool LockYToTarget = true;
    [Tooltip("Décalage vertical appliqué au VFX par rapport à la cible.")]
    public float YOffset = 0f;

    // -------- Orientation par le vent --------
    [Header("Orientation (Wind)")]
    [Tooltip("Oriente ce GameObject selon WindDirection.")]
    public bool AlignToWind = true;
    [Tooltip("Ne tourner qu'autour de Y (horizontal) ; sinon yaw+pitch (pas de roll).")]
    public bool YawOnly = true;
    [Tooltip("L’objet regarde dans le sens du vent (true) ou face au vent (false).")]
    public bool FaceWithWind = true;
    [Tooltip("Facteur de lissage de rotation (0 = instantané).")]
    [Range(0f, 20f)] public float RotationSmoothing = 8f;

    // -------- Water materials --------
    [Header("Ripple Settings")]
    [Range(0, 1)] public float RippleStrength = 0.3f;
    [Range(0, 1)] public float WaterMask = 1f;
    [Range(0, 1)] public float Smoothness = 0f;

    // -------- Rain logic --------
    [Header("Rain Settings")]
    public float RainMaxIntensity = 100000f;    // Cap du spawn VFX
    [Range(0, 1)] public float RainIntensity = 0f; // 0..1 (piloté par météo)

    [Tooltip("Vitesse verticale min/max des gouttes (sans vent)")]
    public Vector3 RainMinVelocity = new Vector3(0, -24, 0);
    public Vector3 RainMaxVelocity = new Vector3(0, -30, 0);

    [Header("Humidity → Rain mapping (%RH)")]
    [Range(0, 100)] public float HumidityRainStart = 70f;
    [Range(0, 100)] public float HumidityRainFull = 95f;
    [Range(1f, 4f)] public float DelugeGamma = 2.5f;
    public float DelugeBoost = 3f;
    [Range(0, 100)] public float DelugeHumidity = 98f;

    [Header("Storm tuning")]
    public float VelocityBoostAtMax = 1.5f;
    public float TurbulenceBase = 0.5f;
    public float TurbulenceAtMax = 4f;

    [Header("Wind (inclinaison des gouttes)")]
    public Vector3 WindDirection = new Vector3(1, 0, 0);
    public float WindSpeed = 0f;                  // m/s “ressenti” jeu
    public float LateralFactorAtMax = 1.0f;       // penchement à pleine intensité

    [Header("Audio")]
    public AudioMixer mixer; // exposer un paramètre "RainVolume" dans le mixer

    // -------- Shader/VFX property IDs --------
    static readonly int _rippleStrengthID = Shader.PropertyToID("_RippleStrength");
    static readonly int _waterMaskID = Shader.PropertyToID("_Mask");
    static readonly int _smoothnessID = Shader.PropertyToID("_Smoothness");

    static readonly int _rateId = Shader.PropertyToID("Rate");
    static readonly int _rainMaxVelocityID = Shader.PropertyToID("Velocity2");
    static readonly int _rainMinVelocityID = Shader.PropertyToID("Velocity1");

    const string VFX_NoiseIntensity = "NoiseIntensity";

    // -------- Internals --------
    const float INTENSITY_ON = 0.03f;  // 3%
    const float INTENSITY_OFF = 0.015f; // 1.5% (hystérésis)
    bool _vfxRunning;

    // --- Helpers ---
    static float Linear01ToDb(float v01) => (v01 <= 0.0001f) ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(v01));
    static float Smooth01(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

    float MapHumidityToRain01(float humidityPercent)
    {
        float t = Mathf.InverseLerp(HumidityRainStart, HumidityRainFull, humidityPercent);
        t = Smooth01(t);
        t = Mathf.Pow(t, 1f / Mathf.Max(1f, DelugeGamma));
        float boost = humidityPercent >= DelugeHumidity ? DelugeBoost : 1f;
        return Mathf.Clamp01(t * boost);
    }

    void Awake()
    {
        _vfxRunning = false;
    }

    void Start()
    {
        RainIntensity = 0f;
        SafeEnablePass(false);
    }

    void Update()
    {
        UpdateMaterials();
        HandleRainRunState();
        DriveVFX();
        DriveAudio();

        // --- Orientation par le vent (rotation du GameObject) ---
        if (AlignToWind)
        {
            ApplyWindRotation();
        }
    }

    void LateUpdate()
    {
        // Verrouillage Y au joueur
        if (LockYToTarget && YFollowTarget)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, YFollowTarget.position.y + YOffset, p.z);
        }

        if (!Application.isPlaying) return;

        // Météo → intensité
        if (WeatherHandlerData.CurrentWeather != null)
        {
            float h = WeatherHandlerData.CurrentWeather.Humidity;
            float humidityPercent = (h <= 1.0001f) ? h * 100f : h;
            RainIntensity = MapHumidityToRain01(humidityPercent);
        }
    }

    // --------- Sections privées ---------

    void UpdateMaterials()
    {
        if (WaterMaterial)
        {
            WaterMaterial.SetFloat(_rippleStrengthID, RippleStrength);
            WaterMaterial.SetFloat(_waterMaskID, WaterMask);
            WaterMaterial.SetFloat(_smoothnessID, Smoothness);
        }
        if (WaterLensMaterial) { /* lens optionnel */ }
    }

    void HandleRainRunState()
    {
        bool wantRunning = _vfxRunning ? (RainIntensity > INTENSITY_OFF)
                                       : (RainIntensity >= INTENSITY_ON);

        if (wantRunning && !_vfxRunning)
        {
            if (RainVFX) { RainVFX.Reinit(); RainVFX.Play(); }
            _vfxRunning = true;
            SafeEnablePass(true);
        }
        else if (!wantRunning && _vfxRunning)
        {
            if (RainVFX)
            {
                RainVFX.SetFloat(_rateId, 0f);
                RainVFX.Stop();
            }
            _vfxRunning = false;
            SafeEnablePass(false);
        }
    }

    void DriveVFX()
    {
        if (!RainVFX) return;

        // ------- Densité / lifetime -------
        float targetAliveMax = 1.2e5f;
        float targetAlive = Mathf.Lerp(0f, targetAliveMax, RainIntensity);

        float lifetime = Mathf.Lerp(4f, 1.2f, RainIntensity);
        if (RainVFX.HasFloat("Lifetime"))
            RainVFX.SetFloat("Lifetime", lifetime);

        float spawnRate = _vfxRunning ? targetAlive / Mathf.Max(0.1f, lifetime) : 0f;
        spawnRate = Mathf.Min(spawnRate, RainMaxIntensity);
        RainVFX.SetFloat(_rateId, spawnRate);

        // ------- Turbulence -------
        float turb = Mathf.Lerp(TurbulenceBase, TurbulenceAtMax, RainIntensity);
        RainVFX.SetFloat(VFX_NoiseIntensity, _vfxRunning ? turb : 0f);

        // ------- Direction / vitesse -------
        if (!_vfxRunning)
        {
            RainVFX.SetVector3(_rainMinVelocityID, Vector3.zero);
            RainVFX.SetVector3(_rainMaxVelocityID, Vector3.zero);
            return;
        }

        // vitesse verticale de base (module)
        float vDownMag = Mathf.Abs(Vector3.Lerp(RainMinVelocity, RainMaxVelocity, Mathf.Clamp01(RainIntensity)).y);
        vDownMag *= Mathf.Lerp(1f, VelocityBoostAtMax, Mathf.Clamp01(RainIntensity));

        // force horizontale du vent (module)
        float pushH = WindSpeed * (LateralFactorAtMax * Mathf.Clamp01(RainIntensity));

        // ---- MODE A : Rotation du GameObject = direction de la pluie (Local space conseillé)
        // Down = -transform.up ; Lateral = transform.forward (cap du vent)
        bool useRotation = AlignToWind; // si tu orientes l'objet via ApplyWindRotation()
        Vector3 downDir, lateralDir;

        if (useRotation)
        {
            downDir = -transform.up;           // chute selon l'orientation du GO
            lateralDir = transform.forward;       // vent dans l'axe avant du GO
        }
        else
        {
            // ---- MODE B : Vecteur de vent explicite (monde)
            Vector3 dirWorld = HeadingDegToDirXZ(
                (UseWeatherWind && Application.isPlaying && WeatherHandlerData.CurrentWeather != null)
                ? WeatherHandlerData.CurrentWeather.WindOrientation
                : WindOrientationDegrees
            );
            if (!FaceWithWind) dirWorld = -dirWorld;
            downDir = Vector3.down;
            lateralDir = dirWorld.normalized;
        }

        Vector3 v = downDir * vDownMag + lateralDir * pushH;

        // Envoie au VFX (si ton graph lit Velocity1/2)
        RainVFX.SetVector3(_rainMinVelocityID, v);
        RainVFX.SetVector3(_rainMaxVelocityID, v);
    }


    void DriveAudio()
    {
        if (!mixer) return;
        float vol01 = _vfxRunning ? RainIntensity : 0f;
        mixer.SetFloat("RainVolume", Linear01ToDb(vol01));
    }

    void SafeEnablePass(bool enable)
    {
        if (_rainVolume) _rainVolume.enabled = enable;
    }

    // --- Wind heading in degrees ---
    public bool UseWeatherWind = true;     // lit WeatherHandlerData si dispo
    [Range(0f, 360f)] public float WindOrientationDegrees = 0f;  // 0=N, 90=E (plan XZ)

    // Référence optionnelle pour un repère "caméra" (si tu veux raisonner gauche/droite par rapport au joueur)
    public Transform OrientationReference; // ex: Camera/Pawn

    public enum OrientationFrame { World, RelativeToReference }
    public OrientationFrame OrientationSpace = OrientationFrame.World;

    // Lissage d’angles (SmoothDampAngle)
    float _yawVel, _pitchVel;
    [Range(0f, 720f)] public float MaxYawDegPerSec = 360f; // limite angulaire
    [Range(0f, 60f)] public float MaxPitchDeg = 25f;  // inclinaison max (réalisme)
    public float YawSmoothTime = 0.15f;
    public float PitchSmoothTime = 0.20f;


    // ---------- Orientation: logique centrale ----------
    void ApplyWindRotation()
    {
        // 1) Récup de l’angle (météo → otherwise l’exposé)
        float headingDeg = WindOrientationDegrees;
        if (UseWeatherWind && WeatherHandlerData.CurrentWeather != null || Application.isPlaying)
            headingDeg = WeatherHandlerData.CurrentWeather.WindOrientation;
        else headingDeg = 0f;

            // 2) Direction monde (XZ)
            Vector3 dirWorld = HeadingDegToDirXZ(headingDeg);
        if (!FaceWithWind) dirWorld = -dirWorld; // face au vent = inverse

        // 3) Si tu veux raisonner "gauche/droite/face" par rapport à la caméra
        if (OrientationSpace == OrientationFrame.RelativeToReference && OrientationReference)
        {
            // Recompose une direction relative (mélange progressif : avant + latéral)
            DecomposeDirRelative(OrientationReference, dirWorld, out float head, out float lat);

            // head ∈ [-1,1] (face/arrière), lat ∈ [-1,1] (gauche/droite)
            Vector3 f = OrientationReference.forward; f.y = 0; f.Normalize();
            Vector3 r = OrientationReference.right; r.y = 0; r.Normalize();

            // Poids progressifs (tu peux jouer sur ces courbes si tu veux biaiser)
            Vector3 blended = f * head + r * lat;
            if (blended.sqrMagnitude > 1e-6f) dirWorld = blended.normalized;
        }

        if (YawOnly) dirWorld.y = 0f;
        if (dirWorld.sqrMagnitude < 1e-6f) return;

        // 4) Cible yaw (plan XZ)
        float targetYaw = Mathf.Atan2(dirWorld.x, dirWorld.z) * Mathf.Rad2Deg; // 0°=Nord
                                                                               // Lissage yaw (SmoothDampAngle + clamp vitesse)
        float currentYaw = transform.eulerAngles.y;
        float rawYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref _yawVel, YawSmoothTime);
        float maxStep = MaxYawDegPerSec * Time.deltaTime;
        float finalYaw = Mathf.MoveTowardsAngle(currentYaw, rawYaw, maxStep);

        // 5) Pitch (inclinaison avant/arrière en fonction de la force horizontale vs chute)
        float horiz = (WindSpeed * LateralFactorAtMax * Mathf.Clamp01(RainIntensity)); // “push” horizontal
        float vert = Mathf.Abs(Vector3.Lerp(RainMinVelocity, RainMaxVelocity, Mathf.Clamp01(RainIntensity)).y);
        float targetPitch = Mathf.Atan2(horiz, Mathf.Max(0.001f, vert)) * Mathf.Rad2Deg;   // 0..~60°
        targetPitch = Mathf.Clamp(targetPitch, 0f, MaxPitchDeg);                            // limite réaliste
                                                                                            // Si FaceWithWind=false on penche vers l’autre sens
        if (!FaceWithWind) targetPitch = -targetPitch;

        float currentPitch = NormalizeAngle180(transform.eulerAngles.x);
        float smPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref _pitchVel, PitchSmoothTime);

        // 6) Applique rotation (sans roll)
        transform.rotation = Quaternion.Euler(smPitch, finalYaw, 0f);
    }

    static float NormalizeAngle180(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        if (a < -180f) a += 360f;
        return a;
    }
    // Convertit un cap (0°=Nord/Z+, 90°=Est/X+) en direction monde sur XZ.
    static Vector3 HeadingDegToDirXZ(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        // Z = cos, X = sin  → 0°=Z+, 90°=X+
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    // Décompose la direction par rapport à un repère: renvoie composantes "devant" et "latéral" (-1..1)
    static void DecomposeDirRelative(Transform reference, Vector3 worldDir, out float head, out float lateral)
    {
        Vector3 f = reference.forward; f.y = 0; f.Normalize();
        Vector3 r = reference.right; r.y = 0; r.Normalize();
        head = Mathf.Clamp(Vector3.Dot(worldDir, f), -1f, 1f); // +1 = vent de face (vers l'avant ref)
        lateral = Mathf.Clamp(Vector3.Dot(worldDir, r), -1f, 1f); // +1 = vent depuis la droite
    }

}
