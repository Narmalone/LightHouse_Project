using UnityEngine;

namespace LightHouse.Game.Boats
{
    /// <summary>
    /// Définition d'une anomalie de bateau (type, nom d'affichage, prefab).
    /// Sert de "factory" pour instancier et attacher l'anomalie à un bateau.
    /// </summary>
    [CreateAssetMenu(menuName = "LightHouse/Boats/Anomaly Definition", fileName = "BoatAnomalyDefinition")]
    public class BoatAnomalyDefinition : ScriptableObject
    {
        #region Serialized Fields

        [Header("Metadata")]
        [Tooltip("Type fonctionnel de l'anomalie (clé de logique).")]
        [SerializeField] private AnomalyType _type;

        [Tooltip("Nom d'affichage lisible par l'utilisateur.")]
        [SerializeField] private string _displayName;

        [Header("Prefab")]
        [Tooltip("Prefab de l'anomalie à instancier.")]
        [SerializeField] private BoatAnomaly _prefab;

        #endregion

        #region Properties

        /// <summary>Type fonctionnel de l'anomalie.</summary>
        public AnomalyType Type => _type;

        /// <summary>Nom d'affichage de l'anomalie.</summary>
        public string DisplayName => _displayName;

        /// <summary>Prefab utilisé pour créer l'instance d'anomalie.</summary>
        public BoatAnomaly Prefab => _prefab;

        #endregion

        #region Public API

        /// <summary>
        /// Instancie l'anomalie et l'attache au bateau.
        /// </summary>
        /// <param name="boat">Bateau cible.</param>
        /// <param name="parent">
        /// Parent optionnel pour l'instance. Si null, on utilise le transform du Rigidbody du bateau.
        /// </param>
        /// <param name="autoApply">
        /// Si true, appelle <see cref="BoatAnomaly.Apply"/> après <see cref="BoatAnomaly.Initialize(Boat)"/>.
        /// </param>
        /// <returns>L'instance créée, ou null en cas d'erreur.</returns>
        public virtual BoatAnomaly InstantiateAndAttach(Boat boat, Transform parent = null, bool autoApply = true)
        {
            if (_prefab == null)
            {
                Debug.LogError($"[{nameof(BoatAnomalyDefinition)}] Prefab manquant sur '{name}'.");
                return null;
            }

            if (boat == null)
            {
                Debug.LogError($"[{nameof(BoatAnomalyDefinition)}] Boat null pour '{name}'.");
                return null;
            }

            var attachParent = parent ?? boat.RB?.transform;
            if (attachParent == null)
            {
                Debug.LogError($"[{nameof(BoatAnomalyDefinition)}] Impossible de trouver un parent (Rigidbody manquant ?) pour '{name}'.");
                return null;
            }

            var instance = Instantiate(_prefab, attachParent);
            instance.Initialize(boat);

            if (autoApply)
                instance.Apply();

            return instance;
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Petits garde-fous éditeur
            if (string.IsNullOrWhiteSpace(_displayName))
                _displayName = _type.ToString();

            if (_prefab == null)
            {
                // Conseil silencieux dans l'inspecteur (pas d'erreur dure ici).
                // Debug.LogWarning($"[{nameof(BoatAnomalyDefinition)}] Assignez un Prefab sur '{name}'.");
            }
        }

        #endregion
    }
}
