using UnityEngine;

namespace LightHouse.Game.Boats
{
    /// <summary>
    /// Classe de base abstraite pour toutes les anomalies de bateau.
    /// Fournit le contrat d'initialisation, d'application et de résolution.
    /// </summary>
    public abstract class BoatAnomaly : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Anomaly Settings")]
        [Tooltip("Gravité de l'anomalie (influence les priorités d'intervention).")]
        [SerializeField, Range(0.1f, 5f)]
        private float _severity = 1f;

        #endregion

        #region Properties

        /// <summary>
        /// Type fonctionnel de l'anomalie.
        /// </summary>
        public abstract AnomalyType Type { get; }

        /// <summary>
        /// Gravité de l'anomalie (0.1 = faible, 5 = critique).
        /// </summary>
        public float Severity => _severity;

        /// <summary>
        /// Référence au bateau affecté.
        /// </summary>
        protected Boat Boat => _boat;

        #endregion

        #region Private Fields

        private Boat _boat;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initialise l'anomalie avec le bateau cible.
        /// </summary>
        /// <param name="boat">Bateau sur lequel l'anomalie est attachée.</param>
        public virtual void Initialize(Boat boat)
        {
            if (boat == null)
            {
                Debug.LogError($"[{nameof(BoatAnomaly)}] Boat fourni est null pour '{name}'.");
                return;
            }

            _boat = boat;
        }

        #endregion

        #region Abstract API

        /// <summary>
        /// Applique l'anomalie (effets visuels, comportementaux, etc.).
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Résout l'anomalie et restaure l'état normal.
        /// </summary>
        public abstract void Resolve();

        #endregion
    }
}
