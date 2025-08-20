using UnityEngine;
using System;

namespace LightHouse.Game.Boats
{
    /// <summary>
    /// Gère la logique d'apparition, de déclenchement et de résolution d'une anomalie pour un bateau.
    /// </summary>
    public class BoatAnomalyController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private float _minAnomalyProgress = 0.1f;
        [SerializeField] private float _maxAnomalyProgress = 0.5f;

        #endregion

        #region Private Fields

        private BoatAnomaly _currentAnomaly;
        private BoatAnomalyDefinition _currentAnomalyDefinition;

        #endregion

        #region Properties

        /// <summary>
        /// Progression aléatoire à laquelle l'anomalie sera déclenchée.
        /// </summary>
        public float AnomalyTriggerProgress { get; private set; }

        /// <summary>
        /// True si l'anomalie a déjà été déclenchée depuis son ajout.
        /// </summary>
        public bool HasBeenTriggered { get; private set; }

        #endregion

        #region Events

        public event Action OnAnomalyAdded;
        public event Action OnAnomalyResolved;

        #endregion

        private void Awake()
        {
            AnomalyTriggerProgress = UnityEngine.Random.Range(_minAnomalyProgress, _maxAnomalyProgress);
            HasBeenTriggered = false;
        }

        #region Public API

        /// <summary>
        /// Prépare une anomalie à apparaître sur ce bateau.
        /// </summary>
        public void AddAnomaly(BoatAnomalyDefinition definition)
        {
            _currentAnomalyDefinition = definition;
            OnAnomalyAdded?.Invoke();
        }

        /// <summary>
        /// Déclenche réellement l'anomalie et l'attache au bateau.
        /// </summary>
        public void TriggerAnomaly(Boat boat)
        {
            if (_currentAnomalyDefinition == null)
                return;

            _currentAnomaly = _currentAnomalyDefinition.InstantiateAndAttach(boat);
            HasBeenTriggered = true;
        }

        /// <summary>
        /// Résout l'anomalie et détruit son instance.
        /// </summary>
        public void RemoveAnomaly()
        {
            if (_currentAnomaly != null)
            {
                Destroy(_currentAnomaly.gameObject);
                _currentAnomaly = null;
            }

            OnAnomalyResolved?.Invoke();
        }

        /// <summary>
        /// Retourne l'instance active de l'anomalie.
        /// </summary>
        public BoatAnomaly GetActiveAnomaly() => _currentAnomaly;

        #endregion
    }
}
