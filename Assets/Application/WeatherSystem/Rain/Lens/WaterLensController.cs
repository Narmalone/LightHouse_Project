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

    [Header("Player state")]
    public bool IsIndoors = false;            // set par volumes/trigger

    [Header("Smoothing")]
    [Tooltip("Vitesse de fondu de l'intensité perçue (0→1)")]
    [SerializeField] private float fadeSpeed = 0.5f;

    // ---- Shader property IDs ----
    static readonly int _isRainingID = Shader.PropertyToID("_isRaining");          // 0..1
    static readonly int _dropAmountID = Shader.PropertyToID("_DropAmount");         // 0..1
    static readonly int _rainDropVelID = Shader.PropertyToID("_RainDropVelocity");   // float
    static readonly int _dripsStrengthID = Shader.PropertyToID("_DripsStrength");    // float
    // (Si ton shader a d'autres noms de propriétés pour tailles/vel drips, ajoute leurs IDs ici)

    // ---------- Mapping ----------
    [Header("Global mapping")]
    [Tooltip("Courbe qui transforme l'intensité de pluie (0..1) en pilotage shader (0..1).")]
    public AnimationCurve response = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Facteur appliqué après la courbe (0..1)")]
    [Range(0f, 2f)] public float globalGain = 1f;

    [Header("Drops (grosses gouttes)")]
    public Vector2 dropAmount01 = new Vector2(0.0f, 1.0f);  // 0..1 → quantité à l’écran
    public Vector2 dropSize01 = new Vector2(0.6f, 1.2f);  // taille des gouttes (si utilisé dans le graph)
    public Vector2 rainDropVel = new Vector2(0.6f, 2.2f);  // vitesse des grosses gouttes
    public Vector2 noiseAmount = new Vector2(30f, 70f);    // bruit/variation (si utilisé)

    [Header("Drips (filets)")]
    public Vector2 dripAmount01 = new Vector2(0.0f, 1.0f);   // 0..1 (si utilisé)
    public Vector2 dripsVelocity = new Vector2(0.3f, 1.2f);   // vitesse des filets (si utilisé)
    public Vector2 dripsStrength = new Vector2(20f, 80f);     // “poids”/épaisseur
    [Tooltip("Taille X/Y des filets (en unités shader)")]
    public Vector2 dripSizeMin = new Vector2(0.5f, 0.5f);
    public Vector2 dripSizeMax = new Vector2(0.9f, 0.9f);

    // Internes
    float _smoothedIntensity;

    private void Awake()
    {
        if (WaterLensMaterial && WaterLensMaterial.HasFloat(_isRainingID))
            _smoothedIntensity = Mathf.Clamp01(WaterLensMaterial.GetFloat(_isRainingID));
    }

    void Update()
    {
        if (!WaterLensMaterial || rainController == null) return;

        IsIndoors = PlayerHandlerData.IsPlayerOccluded();

        // 1) Intensité cible (zéro en intérieur) + lissage
        float target = IsIndoors ? 0f : Mathf.Clamp01(rainController.RainIntensity);
        _smoothedIntensity = Mathf.MoveTowards(_smoothedIntensity, target, fadeSpeed * Time.deltaTime);

        // 2) Ecrit l’état binaire/continu de pluie pour le graph
        SafeSetFloat(_isRainingID, _smoothedIntensity);

        // 3) Deux "t" :
        // - tRaw : directement l'intensité lissée (0..1) — utile pour les vitesses (mapping linéaire min..max)
        // - tCurve : intensité passée dans la courbe (perception/amount) puis globalGain (clampée 0..1)
        float tRaw = _smoothedIntensity;
        float tCurve = Mathf.Clamp01(response.Evaluate(_smoothedIntensity) * globalGain);

        // --- Exemples concrets :
        // a) Amount perçu (courbe) : 0.6 pluie -> amount = Lerp(min, max, tCurve)
        SafeSetFloat(_dropAmountID, Mathf.Lerp(dropAmount01.x, dropAmount01.y, tCurve));

        // b) VITESSES (linéaire à l’intensité) : 0.6 pluie -> vel = Lerp(min, max, 0.6)
        SafeSetFloat(_rainDropVelID, Mathf.Lerp(rainDropVel.x, rainDropVel.y, tRaw));

        // c) Drips strength : tu peux choisir courbe ou linéaire; ici je mets linéaire (sens physique)
        //SafeSetFloat(_dripsStrengthID, Mathf.Lerp(dripsStrength.x, dripsStrength.y, tRaw));

        // NOTE: si tu ajoutes d'autres propriétés shader (taille gouttes/filets, bruit, etc.),
        // applique le même principe :
        //  - perceptif/visuel (densité, quantité) -> tCurve
        //  - "physique" (vitesses) -> tRaw
    }

    // --------- Helpers ---------
    void SafeSetFloat(int id, float value)
    {
        // On fait confiance au shader pour ces IDs (optimisé).
        // Ajoute des bools si tu veux activer/désactiver certains envois.
        WaterLensMaterial.SetFloat(id, value);
    }
}
