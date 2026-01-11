using UnityEngine;
using UnityEngine.VFX;

namespace LightHouse.Features.Boats.Anomalies
{
    /// <summary>
    /// Anomalie représentant un incendie à bord d'un bateau.
    /// </summary>
    public class FireAnomaly : BoatAnomaly
    {
        #region Serialized Fields

        [Header("Fire Anomaly Settings")]
        [Tooltip("Effet visuel représentant l'incendie.")]
        [SerializeField]
        private VisualEffect _fireEffect;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override AnomalyType Type => AnomalyType.FireAboard;

        #endregion

        #region Public API

        /// <inheritdoc/>
        public override void Apply()
        {
            if (_fireEffect == null)
            {
                Debug.LogWarning($"[{nameof(FireAnomaly)}] Aucun effet de feu assigné sur '{name}'.");
                return;
            }

            _fireEffect.Play();
        }

        /// <inheritdoc/>
        public override void Resolve()
        {
            if (_fireEffect != null)
            {
                Destroy(_fireEffect.gameObject);
            }
            else
            {
                Debug.LogWarning($"[{nameof(FireAnomaly)}] Aucun effet de feu à détruire sur '{name}'.");
            }
        }

        #endregion
    }
}
