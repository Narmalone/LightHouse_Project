using LightHouse.Game.World;
using LightHouse.Handlers;
using UnityEngine;

/// <summary>
/// Pilote le shader WaterLens en fonction de l'intensité de pluie et de l'intérieur/extérieur.
/// Permet de borner min/max et d'appliquer une courbe de réponse par-paramètre.
/// </summary>
public class WaterLensController : MonoBehaviour
{
    [Header("Refs")]
    public Material WaterLensMaterial;        // même mat que RainController
    public RainController rainController;     // source de RainIntensity
    public Transform YFollowTarget;

    [Header("Player state")]
    public bool IsIndoors = false;            // set par volumes/trigger
    public LayerMask skyBlockerMask;

    [Header("Smoothing")]
    [Tooltip("Vitesse de fondu de l'intensité perçue (0→1)")]
    [SerializeField] private float fadeSpeed = 0.5f;

    // ---- Shader property IDs ----
    static readonly int _isRainingID = Shader.PropertyToID("_isRaining");        // 0..1
    static readonly int _dropAmountID = Shader.PropertyToID("_DropAmount");       // 0..1
    static readonly int _dropSizeID = Shader.PropertyToID("_DropSize");         // float
    static readonly int _rainDropVelID = Shader.PropertyToID("_RainDropVelocity"); // float
    static readonly int _noiseID = Shader.PropertyToID("_Noise");            // float
    static readonly int _dripAmountID = Shader.PropertyToID("_DripAmount");       // 0..1
    static readonly int _dripsVelID = Shader.PropertyToID("_DripsVelocity");    // float
    static readonly int _dripsStrengthID = Shader.PropertyToID("_DripsStrength");    // float
    static readonly int _dripSizeVecID = Shader.PropertyToID("_DripSize");         // Vector2 (ShaderGraph → Vector/Color)

    // ---------- Mapping ----------
    [Header("Global mapping")]
    [Tooltip("Courbe qui transforme l'intensité de pluie (0..1) en pilotage shader (0..1).")]
    public AnimationCurve response = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Facteur appliqué après la courbe (0..1)")]
    [Range(0f, 2f)] public float globalGain = 1f;

    [Header("Drops (grosses gouttes)")]
    public Vector2 dropAmount01 = new Vector2(0.0f, 1.0f);  // 0..1 → quantité à l’écran
    public Vector2 dropSize01 = new Vector2(0.6f, 1.2f);  // taille des gouttes
    public Vector2 rainDropVel = new Vector2(0.6f, 2.2f);  // vitesse descente des grosses gouttes
    public Vector2 noiseAmount = new Vector2(30f, 70f);    // bruit/variation (si utilisé dans le graph)

    [Header("Drips (filets)")]
    public Vector2 dripAmount01 = new Vector2(0.0f, 1.0f);  // 0..1
    public Vector2 dripsVelocity = new Vector2(0.3f, 1.2f);  // vitesse des filets
    public Vector2 dripsStrength = new Vector2(20f, 80f);    // “poids”/épaisseur
    [Tooltip("Taille X/Y des filets (en unités shader)")]
    public Vector2 dripSizeMin = new Vector2(0.5f, 0.5f);
    public Vector2 dripSizeMax = new Vector2(0.9f, 0.9f);

    // Internes
    float _smoothedIntensity;

    private void Awake()
    {
        //GameZoneHandlerData.OnGameZoneChanged += OnZoneChanged;
        PlayerHandlerData.OnHandlerInitialized += PlayerHandlerData_OnHandlerInitialized;
        // init depuis le matériel (pratique si on a une valeur préexistante)
        if (WaterLensMaterial && WaterLensMaterial.HasFloat(_isRainingID))
            _smoothedIntensity = Mathf.Clamp01(WaterLensMaterial.GetFloat(_isRainingID));
    }

    private void PlayerHandlerData_OnHandlerInitialized()
    {
        if (YFollowTarget == null && PlayerHandlerData.MainPlayer != null)
            YFollowTarget = PlayerHandlerData.MainPlayer.Character.transform;
    }

    private void OnDestroy()
    {
        //GameZoneHandlerData.OnGameZoneChanged -= OnZoneChanged;
        PlayerHandlerData.OnHandlerInitialized -= PlayerHandlerData_OnHandlerInitialized;
    }

    private void OnZoneChanged(ZoneType zone)
    {
        IsIndoors = (zone == ZoneType.Inside);
    }

    void Update()
    {
        if (!WaterLensMaterial || rainController == null) return;

        IsIndoors = IsOccluded();

        // 1) Intensité “cible” (zéro en intérieur) + lissage
        float target = IsIndoors ? 0f : Mathf.Clamp01(rainController.RainIntensity);
        _smoothedIntensity = Mathf.MoveTowards(_smoothedIntensity, target, fadeSpeed * Time.deltaTime);

        // 2) On renseigne _isRaining pour ton graph (si utilisé)
        SafeSetFloat(_isRainingID, _smoothedIntensity);

        // 3) Calcul du pilotage via la courbe globale
        float t = Mathf.Clamp01(response.Evaluate(_smoothedIntensity) * globalGain);

        // 4) Appliquer param par param (lerp sur [min..max] avec t)
        // Drops
        SafeSetFloat(_dropAmountID, Mathf.Lerp(dropAmount01.x, dropAmount01.y, t));
        //SafeSetFloat(_dropSizeID, Mathf.Lerp(dropSize01.x, dropSize01.y, t));
        //SafeSetFloat(_rainDropVelID, Mathf.Lerp(rainDropVel.x, rainDropVel.y, t));
        //SafeSetFloat(_noiseID, Mathf.Lerp(noiseAmount.x, noiseAmount.y, t));

        // Drips
        SafeSetFloat(_dripAmountID, Mathf.Lerp(dripAmount01.x, dripAmount01.y, t));
        //SafeSetFloat(_dripsVelID, Mathf.Lerp(dripsVelocity.x, dripsVelocity.y, t));
        SafeSetFloat(_dripsStrengthID, Mathf.Lerp(dripsStrength.x, dripsStrength.y, t));

        // Vector2 (ShaderGraph range) — souvent stocké comme Vector4/Color
       /* Vector2 dsz = new Vector2(
            Mathf.Lerp(dripSizeMin.x, dripSizeMax.x, t),
            Mathf.Lerp(dripSizeMin.y, dripSizeMax.y, t)
        );
        SafeSetVector2(_dripSizeVecID, dsz);*/
    }

    bool IsOccluded()
    {
        if (!YFollowTarget) return false;
        Vector3 origin = YFollowTarget.position + Vector3.up * 0.1f;
        // Ray haut jusqu’à “loin”
        return Physics.Raycast(origin, Vector3.up, 100f, skyBlockerMask, QueryTriggerInteraction.Ignore);
    }

    // --------- Helpers de set "safe" (si la prop n'existe pas, on ignore) ---------
    void SafeSetFloat(int id, float value)
    {
        // Pas d’API HasFloat par ID : on teste par nom si besoin
        // Ici on fait confiance au shader; si tu veux être ultra-safe,
        // ajoute une liste de bools 'enableX' pour n'envoyer que ce que tu utilises.
        WaterLensMaterial.SetFloat(id, value);
    }

    void SafeSetVector2(int id, Vector2 v)
    {
        // Shader Graph stocke souvent Vector2 en Vector4/Color
        // SetVector fonctionne pour les deux cas.
        WaterLensMaterial.SetVector(id, new Vector4(v.x, v.y, 0f, 0f));
    }

    // API pratique si tu veux forcer indoor/outdoor par script
    public void SetIndoors(bool indoors) => IsIndoors = indoors;
}
