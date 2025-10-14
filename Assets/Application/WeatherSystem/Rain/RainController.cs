using LightHouse.Weather;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;
using UnityEngine.Rendering.HighDefinition;
using LightHouse.Handlers;

[ExecuteInEditMode]
public class RainController : MonoBehaviour
{
    //==============================================================
    #region ► Exposed references (materials, VFX, render passes)
    //==============================================================
    [Header("Materials & VFX")]
    public Material WaterMaterial;
    public Material WaterLensMaterial;
    public VisualEffect RainVFX;
    [SerializeField] private CustomPassVolume _rainVolume;
    #endregion

    //==============================================================
    #region ► Pluie (intensité, vitesses, turbulence)
    //==============================================================
    [Header("Rain Settings")]
    public float RainMaxIntensity = 100000f;     // Cap du spawn VFX
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
    #endregion


    #region ► Follow / Placement (verrouillage en Y)
    //==============================================================
    [Header("Follow (VFX position)")]
    [Tooltip("La cible dont on veut caler l'altitude (souvent la caméra ou le joueur).")]
    public Transform YFollowTarget;
    [Tooltip("Si activé, on verrouille la position Y du GameObject au Y de la cible + offset.")]
    public bool LockYToTarget = true;
    [Tooltip("Décalage vertical appliqué au VFX par rapport à la cible.")]
    public float YOffset = 0f;
    #endregion

    //==============================================================
    #region ► Orientation générale (si on VEUT utiliser la rotation du GO)
    //==============================================================
    [Header("Orientation (Wind)")]
    [Tooltip("Si TRUE, on oriente ce GameObject pour diriger la pluie. Si FALSE, on suit l'angle de vent du jeu (recommandé).")]
    public bool AlignToWind = false; // ← pour un comportement réaliste basé sur l'orientation météo, laisse FALSE
    [Tooltip("Ne tourner qu'autour de Y (horizontal) ; sinon yaw+pitch (pas de roll).")]
    public bool YawOnly = true;
    [Tooltip("L’objet regarde dans le sens du vent (true) ou face au vent (false).")]
    public bool FaceWithWind = true;
    [Tooltip("Facteur de lissage de rotation (0 = instantané).")]
    [Range(0f, 20f)] public float RotationSmoothing = 8f;

    // Limites d'angle (optionnelles) si tu utilises AlignToWind = TRUE
    [Header("Yaw angle clamps (only if AlignToWind = TRUE)")]
    [Range(0f, 180f)] public float MaxYawRightDeg = 0f;
    [Range(0f, 180f)] public float MaxYawLeftDeg = 0f;

    // internes pour le lissage d'angle (si AlignToWind = TRUE)
    float _yawVel, _pitchVel;
    [Range(0f, 720f)] public float MaxYawDegPerSec = 360f;
    [Range(0f, 60f)] public float MaxPitchDeg = 25f;
    public float YawSmoothTime = 0.15f;
    public float PitchSmoothTime = 0.20f;
    #endregion

    //==============================================================
    #region ► Vent (orientation, réponse latérale et clamps)
    //==============================================================
    [Header("Wind (orientation & lateral response)")]
    [Tooltip("Direction du vent simple (utilisée uniquement si AlignToWind=false ET pas de WeatherData).")]
    public Vector3 WindDirection = new Vector3(1, 0, 0);

    [Tooltip("Vitesse de vent (m/s). Si ton jeu stocke en km/h, convertis → m/s = km/h * 0.27778.")]
    public float WindSpeed = 0f;

    [Tooltip("Multiplicateur de 'penchement' à pleine intensité.")]
    public float LateralFactorAtMax = 1.0f;

    [Header("Lateral control")]
    [Tooltip("Réponse globale de la pluie au vent (1 = nominal).")]
    public float LateralResponse = 1f;

    [Tooltip("Vitesse latérale max (m/s), 0 = pas de cap absolu.")]
    public float MaxLateralSpeed = 0f;

    [Tooltip("Inclinaison max autorisée (deg). Limite physique : vLat ≤ tan(angle)*vDown.")]
    [Range(0f, 80f)] public float MaxLeanAngleDeg = 20f;

    [Tooltip("Courbe de réponse : WindSpeed (m/s) → facteur [0..1]. Vide = linéaire (10 m/s ~ 1).")]
    public AnimationCurve WindToLateral;

    [Tooltip("Temps de lissage de la vitesse envoyée au VFX.")]
    [Range(0f, 0.5f)] public float VelocitySmoothTime = 0.08f;

    [Header("Lateral clamps (right/left)")]
    [Tooltip("Cap m/s de la poussée horizontale côté DROIT (+X local). 0 = pas de cap.")]
    public float MaxPushRight = 0f;
    [Tooltip("Cap m/s de la poussée horizontale côté GAUCHE (-X local). 0 = pas de cap.")]
    public float MaxPushLeft = 0f;

    // internes pour le lissage de la vitesse envoyée au VFX
    Vector3 _velCurrent;
    Vector3 _velDamp;
    #endregion

    //==============================================================
    #region ► Audio pluie
    //==============================================================
    [Header("Audio")]
    public AudioMixer mixer; // exposer un paramètre "RainVolume" dans le mixer

    [Header("Audio Sources (loops)")]
    public AudioSource loopLight;
    public AudioSource loopMedium;
    public AudioSource loopHeavy;
    public AudioSource loopWind;

    // Courbes designer → 0..1 (RainIntensity) -> 0..1 (linéaire avant conversion dB)
    [Header("Curves (intensity → layer)")]
    public AnimationCurve volLight = AnimationCurve.Linear(0, 0, 0.4f, 1);
    public AnimationCurve volMedium = AnimationCurve.Linear(0.2f, 0, 0.8f, 1);
    public AnimationCurve volHeavy = AnimationCurve.Linear(0.6f, 0, 1f, 1);

    // LPF & Reverb (0..1 → Hz / dB)
    [Header("FX Curves")]
    public AnimationCurve lpfCurve = new AnimationCurve(
        new Keyframe(0f, 12000f), new Keyframe(1f, 1800f)
    );
    public AnimationCurve reverbCurve = new AnimationCurve(
        new Keyframe(0f, -30f), new Keyframe(1f, 0f)
    );

    // Vent → bruit
    [Header("Wind Noise")]
    public AnimationCurve windVolFromSpeed = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(10f, 1f), new Keyframe(25f, 1.2f)
    );
    [Range(0f, 1f)] public float windWetFactor = 0.7f; // pondère par la pluie

    // Occlusion/Indoor
    [Header("Occlusion (roof check)")]
    public LayerMask skyBlockerMask;
    public float occlusionLPFMultiplier = 0.6f;  // 60% cutoff si occlus
    public float occlusionReverbDb = -10f;       // moins de reverb sous abri

    // Smoothing
    [Header("Smoothing")]
    [Range(0f, 0.5f)] public float volSmooth = 0.08f;
    float _vL, _vM, _vH, _vWind;
    float _dL, _dM, _dH, _dWind;

    #endregion

    //==============================================================
    #region ► Matériaux eau (ripple / lens)
    //==============================================================
    [Header("Ripple Settings")]
    [Range(0, 1)] public float RippleStrength = 0.3f;
    [Range(0, 1)] public float WaterMask = 1f;
    [Range(0, 1)] public float Smoothness = 0f;

    // IDs shader
    static readonly int _rippleStrengthID = Shader.PropertyToID("_RippleStrength");
    static readonly int _waterMaskID = Shader.PropertyToID("_Mask");
    static readonly int _smoothnessID = Shader.PropertyToID("_Smoothness");
    #endregion

    //==============================================================
    #region ► VFX property IDs & internes
    //==============================================================
    static readonly int _rateId = Shader.PropertyToID("Rate");
    static readonly int _rainMaxVelocityID = Shader.PropertyToID("Velocity2");
    static readonly int _rainMinVelocityID = Shader.PropertyToID("Velocity1");
    const string VFX_NoiseIntensity = "NoiseIntensity";

    const float INTENSITY_ON = 0.03f;   // seuil d’allumage (3%)
    const float INTENSITY_OFF = 0.015f;  // seuil d’extinction (1.5%)
    bool _vfxRunning;

    [Header("Emitter anchoring")]
    [Tooltip("Décale automatiquement l’émetteur au vent pour que la pluie traverse le joueur.")]
    public bool AutoUpwindAnchor = true;
    [Tooltip("Marge de sécurité ajoutée au déplacement upwind (mètres).")]
    public float UpwindPadding = 0.0f;
    #endregion

    //==============================================================
    #region ► Helpers/Utils
    //==============================================================
    static float Linear01ToDb(float v01) => (v01 <= 0.0001f) ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(v01));
    static float Smooth01(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }
    static float ClampHz(float hz) => Mathf.Clamp(hz, 20f, 22000f);

    float MapHumidityToRain01(float humidityPercent)
    {
        float t = Mathf.InverseLerp(HumidityRainStart, HumidityRainFull, humidityPercent);
        t = Smooth01(t);
        t = Mathf.Pow(t, 1f / Mathf.Max(1f, DelugeGamma));
        float boost = humidityPercent >= DelugeHumidity ? DelugeBoost : 1f;
        return Mathf.Clamp01(t * boost);
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
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    // Renvoie la direction monde du vent (XZ), en lisant WeatherData si dispo
    Vector3 EvaluateWindDirWorldXZ()
    {
        // 1) Angle météo si disponible
        if (Application.isPlaying && WeatherHandlerData.CurrentWeather != null)
        {
            float headingDeg = WeatherHandlerData.CurrentWeather.WindOrientation;
            Vector3 dir = HeadingDegToDirXZ(headingDeg);
            return FaceWithWind ? dir : -dir;
        }

        // 2) Sinon, fallback sur le champ WindDirection
        Vector3 d = WindDirection;
        d.y = 0f;
        if (d.sqrMagnitude < 1e-6f) d = Vector3.forward; // Nord par défaut
        d.Normalize();
        return FaceWithWind ? d : -d;
    }

    /// <summary>
    /// Calcule les 2 axes de mouvement:
    /// downDir = direction de chute (verticale),
    /// lateralDir = direction horizontale du vent.
    /// - Si AlignToWind = TRUE → basés sur la rotation du GO (local space).
    /// - Sinon → basés sur l’orientation météo (world space).
    /// </summary>
    void GetMotionDirs(out Vector3 downDir, out Vector3 lateralDir)
    {
        if (AlignToWind)
        {
            downDir = -transform.up;
            lateralDir = transform.forward;
        }
        else
        {
            downDir = Vector3.down;
            lateralDir = EvaluateWindDirWorldXZ(); // toujours sur XZ
        }
    }
    #endregion

    //==============================================================
    #region ► Unity lifecycle
    //==============================================================
    void Awake() { _vfxRunning = false; }
    void Start() 
    {
        RainIntensity = 0f; SafeEnablePass(false);
        if (PlayerHandlerData.MainPlayer != null)
            YFollowTarget = PlayerHandlerData.MainPlayer.Character.transform;
    }

    void Update()
    {
        UpdateMaterials();
        HandleRainRunState();
        DriveVFX();
        DriveAudio();

        // Si tu tiens à voir l’objet tourner (mode décoratif), garde AlignToWind = TRUE.
        if (AlignToWind)
        {
            ApplyWindRotation();
            if (AutoUpwindAnchor) AnchorEmitterUpwind();
        }
        else if (AutoUpwindAnchor)
        {
            // Même avec AlignToWind=FALSE, on peut ancrer l’émetteur selon l’orientation météo.
            AnchorEmitterUpwind();
        }
    }

    void LateUpdate()
    {
        // On garde le Y du VFX à la même hauteur que la cible (caméra/joueur)
        if (LockYToTarget && YFollowTarget)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, YFollowTarget.position.y + YOffset, p.z);
        }

        // Mettre à jour RainIntensity depuis la météo, si elle existe
        if (!Application.isPlaying) return;
        if (WeatherHandlerData.CurrentWeather != null)
        {
            float h = WeatherHandlerData.CurrentWeather.Humidity;
            float humidityPercent = (h <= 1.0001f) ? h * 100f : h;
            RainIntensity = MapHumidityToRain01(humidityPercent);
            WindSpeed = WeatherHandlerData.CurrentWeather.WindSpeed;
        }
    }
    #endregion

    //==============================================================
    #region ► Materials / Audio / VFX run-state
    //==============================================================
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

    void DriveAudio()
    {
        if (!Application.isPlaying) return;
        float t = Mathf.Clamp01(RainIntensity);

        // Fallback si curves nulles / vides
        float Eval(AnimationCurve c, float x, float def = 0f)
            => (c != null && c.keys != null && c.keys.Length > 0) ? c.Evaluate(x) : def;

        float light01 = Mathf.Clamp01(Eval(volLight, t));
        float med01 = Mathf.Clamp01(Eval(volMedium, t));
        float heavy01 = Mathf.Clamp01(Eval(volHeavy, t));

        // Soft cap optionnel (ex: 1.2) pour garder un peu d'énergie
        float sum = light01 + med01 + heavy01;
        float cap = 1.0f; // passe à 1.2f si tu préfères
        if (sum > cap) { float k = cap / sum; light01 *= k; med01 *= k; heavy01 *= k; }

        _vL = (volSmooth > 0f) ? Mathf.SmoothDamp(_vL, light01, ref _dL, volSmooth) : light01;
        _vM = (volSmooth > 0f) ? Mathf.SmoothDamp(_vM, med01, ref _dM, volSmooth) : med01;
        _vH = (volSmooth > 0f) ? Mathf.SmoothDamp(_vH, heavy01, ref _dH, volSmooth) : heavy01;

        float windBase01 = Mathf.Max(0f, Eval(windVolFromSpeed, WindSpeed));
        float wind01 = Mathf.Clamp01(windBase01 * Mathf.Lerp(0.2f, 1f, t * windWetFactor));
        _vWind = (volSmooth > 0f) ? Mathf.SmoothDamp(_vWind, wind01, ref _dWind, volSmooth) : wind01;

        // Occlusion (tu peux ne le faire que toutes les N frames)
        bool occluded = IsOccluded();
        Debug.Log(occluded);
        float lpf = ClampHz(Eval(lpfCurve, t, 12000f) * (occluded ? occlusionLPFMultiplier : 1f));
        float reverbDb = Eval(reverbCurve, t, -24f) + (occluded ? occlusionReverbDb : 0f);

        if (mixer)
        {
            if (_vL <= 0f) _vL = 0f;
            if (_vM <= 0f) _vM = 0f;
            if (_vH <= 0f) _vH = 0f;
            if (_vWind <= 0f) _vWind = 0f;
            mixer.SetFloat("Rain_Light", Linear01ToDb(_vL));
            mixer.SetFloat("Rain_Med", Linear01ToDb(_vM));
            mixer.SetFloat("Rain_Heavy", Linear01ToDb(_vH));
            mixer.SetFloat("Rain_Wind", Linear01ToDb(_vWind));
            mixer.SetFloat("Rain_LPF_Cutoff", lpf);
        }

        ManageLoop(loopLight, _vL);
        ManageLoop(loopMedium, _vM);
        ManageLoop(loopHeavy, _vH);
        ManageLoop(loopWind, _vWind);
    }

    // Exemple : ScaleRainCutoff(0.1f); // divise par 10
    public void ScaleRainCutoff(float factor)
    {
        if (!mixer) return;
        if (mixer.GetFloat("Rain_LPF_Cutoff", out float hz))
        {
            float newHz = Mathf.Clamp(hz * factor, 20f, 22000f);
            mixer.SetFloat("Rain_LPF_Cutoff", newHz);
        }
    }

    bool IsOccluded()
    {
        if (!YFollowTarget) return false;
        Vector3 origin = YFollowTarget.position + Vector3.up * 0.1f;
        // Ray haut jusqu’à “loin”
        return Physics.Raycast(origin, Vector3.up, 100f, skyBlockerMask, QueryTriggerInteraction.Ignore);
    }

    void ManageLoop(AudioSource src, float v01)
    {
        if (!src) return;
        if (v01 > 0.01f)
        {
            if (!src.isPlaying) src.Play();
            src.volume = Mathf.Clamp01(v01); // garde volume local modeste, le gros mix se fait dans le mixer
        }
        else if (src.isPlaying)
        {
            src.Stop();
        }
        else
        {
            src.Stop();
        }
    }


    void SafeEnablePass(bool enable)
    {
        if (_rainVolume) _rainVolume.enabled = enable;
    }
    #endregion

    //==============================================================
    #region ► Physique/Direction des gouttes (le cœur)
    //==============================================================
    void DriveVFX()
    {
        if (!RainVFX) return;

        // --- Densité / lifetime
        float targetAliveMax = 1.2e5f;
        float targetAlive = Mathf.Lerp(0f, targetAliveMax, RainIntensity);

        float lifetime = Mathf.Lerp(4f, 1.2f, RainIntensity);
        if (RainVFX.HasFloat("Lifetime"))
            RainVFX.SetFloat("Lifetime", lifetime);

        float spawnRate = _vfxRunning ? targetAlive / Mathf.Max(0.1f, lifetime) : 0f;
        spawnRate = Mathf.Min(spawnRate, RainMaxIntensity);
        RainVFX.SetFloat(_rateId, spawnRate);

        // --- Turbulence
        float turb = Mathf.Lerp(TurbulenceBase, TurbulenceAtMax, RainIntensity);
        RainVFX.SetFloat(VFX_NoiseIntensity, _vfxRunning ? turb : 0f);

        // --- Vitesse/Direction
        if (!_vfxRunning)
        {
            _velCurrent = Vector3.zero;
            RainVFX.SetVector3(_rainMinVelocityID, Vector3.zero);
            RainVFX.SetVector3(_rainMaxVelocityID, VectorBoxZero());
            return;
        }

        float tRain = Mathf.Clamp01(RainIntensity);
        float vDown = Mathf.Abs(Vector3.Lerp(RainMinVelocity, RainMaxVelocity, tRain).y);
        vDown *= Mathf.Lerp(1f, VelocityBoostAtMax, tRain);

        // Réponse au vent (0..1)
        float wind_ms = WindSpeed; // si tu reçois du km/h: wind_ms = WindSpeed * 0.27778f;
        float resp = (WindToLateral != null && WindToLateral.keys.Length > 0)
            ? Mathf.Clamp01(WindToLateral.Evaluate(wind_ms))
            : Mathf.Clamp01(wind_ms / 10f); // ≈ linéaire, 10 m/s ~ fort vent

        float pushH = wind_ms * LateralFactorAtMax * tRain * LateralResponse * resp;

        // Limite physique par l'angle max
        if (MaxLeanAngleDeg > 0.01f)
        {
            float maxByAngle = Mathf.Tan(MaxLeanAngleDeg * Mathf.Deg2Rad) * vDown;
            pushH = Mathf.Min(pushH, maxByAngle);
        }

        // Axes de mouvement (down/lateral) — cohérents partout
        GetMotionDirs(out Vector3 downDir, out Vector3 lateralDir);

        // Horizontal brut
        Vector3 horiz = lateralDir * pushH;

        // Clamp gauche/droite indépendants (par rapport à l'axe right du GO)
        Vector3 rightAxis = transform.right; rightAxis.y = 0f;
        if (rightAxis.sqrMagnitude > 1e-6f) rightAxis.Normalize();

        float sideMag = Vector3.Dot(horiz, rightAxis);   // +droite / -gauche
        Vector3 forwardRest = horiz - rightAxis * sideMag;     // conserve avant/arrière

        if (sideMag >= 0f && MaxPushRight > 0f) sideMag = Mathf.Min(sideMag, MaxPushRight);
        if (sideMag < 0f && MaxPushLeft > 0f) sideMag = Mathf.Max(sideMag, -MaxPushLeft);

        Vector3 clampedHoriz = rightAxis * sideMag + forwardRest;

        // Cap absolu global (optionnel)
        if (MaxLateralSpeed > 0f && clampedHoriz.magnitude > MaxLateralSpeed)
            clampedHoriz = clampedHoriz.normalized * MaxLateralSpeed;

        Vector3 vDesired = downDir * vDown + clampedHoriz;

        // Lissage pour éviter les à-coups
        _velCurrent = (VelocitySmoothTime > 0f)
            ? Vector3.SmoothDamp(_velCurrent, vDesired, ref _velDamp, VelocitySmoothTime)
            : vDesired;

        RainVFX.SetVector3(_rainMinVelocityID, _velCurrent);
        RainVFX.SetVector3(_rainMaxVelocityID, _velCurrent);
    }

    // Utilitaire minuscule pour éviter une allocation dans le return ci-dessus
    static Vector3 VectorBoxZero() => Vector3.zero;
    #endregion

    //==============================================================
    #region ► Ancrage de l’émetteur (éviter que la pluie “passe devant/derrière”)
    //==============================================================
    void AnchorEmitterUpwind()
    {
        if (!YFollowTarget) return;

        // Directions cohérentes avec DriveVFX
        GetMotionDirs(out Vector3 downDir, out Vector3 lateralDir);

        float t = Mathf.Clamp01(RainIntensity);
        float vDown = Mathf.Abs(Vector3.Lerp(RainMinVelocity, RainMaxVelocity, t).y);
        vDown *= Mathf.Lerp(1f, VelocityBoostAtMax, t);

        float wind_ms = WindSpeed;
        float resp = (WindToLateral != null && WindToLateral.keys.Length > 0)
            ? Mathf.Clamp01(WindToLateral.Evaluate(wind_ms))
            : Mathf.Clamp01(wind_ms / 10f);

        float pushH = wind_ms * LateralFactorAtMax * t * LateralResponse * resp;

        // Limite par angle
        if (MaxLeanAngleDeg > 0.01f)
        {
            float maxByAngle = Mathf.Tan(MaxLeanAngleDeg * Mathf.Deg2Rad) * vDown;
            pushH = Mathf.Min(pushH, maxByAngle);
        }

        // Temps de chute réel (projection verticale monde)
        float spawnHeightWorld = Mathf.Max(0.01f, transform.position.y - (YFollowTarget.position.y + YOffset));
        float vDownWorld = Mathf.Max(0.01f, Mathf.Abs(downDir.y) * vDown);
        float fallTime = spawnHeightWorld / vDownWorld;

        // Drift attendu pendant la chute
        Vector3 drift = lateralDir; drift.y = 0f;
        if (drift.sqrMagnitude > 1e-6f) drift.Normalize();
        drift *= (pushH * fallTime + UpwindPadding);

        // Place l'émetteur upwind en XZ
        Vector3 p = transform.position;
        transform.position = new Vector3(
            YFollowTarget.position.x - drift.x,
            p.y,
            YFollowTarget.position.z - drift.z
        );
    }
    #endregion

    //==============================================================
    #region ► Option : rotation décorative si AlignToWind = TRUE
    //==============================================================
    void ApplyWindRotation()
    {
        // NB: Cette rotation n'est utilisée QUE si AlignToWind = TRUE.
        Vector3 dirWorld = EvaluateWindDirWorldXZ();
        if (YawOnly) dirWorld.y = 0f;
        if (dirWorld.sqrMagnitude < 1e-6f) return;

        float targetYaw = Mathf.Atan2(dirWorld.x, dirWorld.z) * Mathf.Rad2Deg;

        // Clamp d'angle gauche/droite (optionnel)
        if (MaxYawRightDeg > 0f || MaxYawLeftDeg > 0f)
        {
            float refYaw = transform.eulerAngles.y;
            float delta = Mathf.DeltaAngle(refYaw, targetYaw);
            float clamped = Mathf.Clamp(delta, -MaxYawLeftDeg, MaxYawRightDeg);
            targetYaw = refYaw + clamped;
        }

        // Lissage yaw
        float currentYaw = transform.eulerAngles.y;
        float rawYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref _yawVel, YawSmoothTime);
        float maxStep = MaxYawDegPerSec * Time.deltaTime;
        float finalYaw = Mathf.MoveTowardsAngle(currentYaw, rawYaw, maxStep);

        // Pitch basé sur ratio vent/chute (limité)
        float horiz = WindSpeed * LateralFactorAtMax * Mathf.Clamp01(RainIntensity);
        float vert = Mathf.Abs(Vector3.Lerp(RainMinVelocity, RainMaxVelocity, Mathf.Clamp01(RainIntensity)).y);
        float targetPitch = Mathf.Atan2(horiz, Mathf.Max(0.001f, vert)) * Mathf.Rad2Deg;
        targetPitch = Mathf.Clamp(targetPitch, 0f, MaxPitchDeg);
        if (!FaceWithWind) targetPitch = -targetPitch;

        float currentPitch = NormalizeAngle180(transform.eulerAngles.x);
        float smPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref _pitchVel, PitchSmoothTime);

        transform.rotation = Quaternion.Euler(smPitch, finalYaw, 0f);
    }
    #endregion
}
