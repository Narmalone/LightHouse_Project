using LightHouse.Core.Player;
using UnityEngine;

namespace LightHouse.Features.Weather.Rain.Lens
{
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
        static readonly int _isRainingID = Shader.PropertyToID("_isRaining");
        static readonly int _dropAmountID = Shader.PropertyToID("_DropAmount");
        static readonly int _rainDropVelID = Shader.PropertyToID("_RainDropVelocity");

        // ---------- Mapping ----------
        [Header("Global mapping")]
        [Tooltip("Courbe qui transforme l'intensité de pluie (0..1) en pilotage shader (0..1).")]
        public AnimationCurve response = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("Facteur appliqué après la courbe (0..1)")]
        [Range(0f, 2f)] public float globalGain = 1f;

        [Header("Drops (grosses gouttes)")]
        public Vector2 dropAmount01 = new Vector2(0.0f, 1.0f);  // quantité visuelle
        public Vector2 rainDropVel = new Vector2(0.6f, 2.2f);  // X=min, Y=max vitesse
        public Vector2 noiseAmount = new Vector2(30f, 70f);    // optionnel

        [Header("Drips (filets)")]
        public Vector2 dripAmount01 = new Vector2(0.0f, 1.0f);
        public Vector2 dripsVelocity = new Vector2(0.3f, 1.2f);

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

            // Vérifie si le joueur est à l'intérieur
            IsIndoors = PlayerHandlerData.IsPlayerOccluded();

            // 1️⃣ Intensité cible (0 si intérieur)
            float target = IsIndoors ? 0f : Mathf.Clamp01(rainController.RainIntensity);
            _smoothedIntensity = Mathf.MoveTowards(_smoothedIntensity, target, fadeSpeed * Time.deltaTime);

            // 2️⃣ Valeurs dérivées
            float tCurve = Mathf.Clamp01(response.Evaluate(_smoothedIntensity) * globalGain);

            // 3️⃣ Envoi au shader
            SafeSetFloat(_isRainingID, _smoothedIntensity);

            // a) Quantité visuelle : courbe (perception)
            SafeSetFloat(_dropAmountID, Mathf.Lerp(dropAmount01.x, dropAmount01.y, tCurve));

            // b) Vélocité brute : pas de lerp → on choisit directement min ou max
            float velocityTarget = 0f;
            if (!IsIndoors)
            {
                velocityTarget = _smoothedIntensity >= 1f
                    ? rainDropVel.y          // pluie max
                    : (_smoothedIntensity <= 0f
                        ? rainDropVel.x      // pluie quasi nulle
                        : rainDropVel.y);    // valeur brute: toujours max si dehors et pluie active
            }

            SafeSetFloat(_rainDropVelID, velocityTarget);
        }

        // --------- Helpers ---------
        void SafeSetFloat(int id, float value)
        {
            if (WaterLensMaterial != null)
                WaterLensMaterial.SetFloat(id, value);
        }
    }

}
