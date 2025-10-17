using LightHouse.Game.Signals;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightHouse.Game.Buyoncies
{
    /// <summary>
    /// Représente les données d'une anomalie sur une bouée.
    /// </summary>
    [Serializable]
    public class BuyoncyAnomalyDatas : ISignal
    {
        public int ID;
        public float RemainingTime { get; set; }
        public string DisplayText { get; set; }

        /// <summary>
        /// Clé unique du signal (format brut de l'ID).
        /// </summary>
        public string Key => ID.ToString();
    }

    /// <summary>
    /// Base de données des anomalies de bouées.
    /// Gère l'ajout, la suppression, la mise à jour et l'expiration automatique.
    /// </summary>
    [CreateAssetMenu(fileName = "BuyoncyAnomalyDatabase", menuName = "LightHouse/Buyoncies/New Database")]
    public class BuyoncyAnomalyDatabase : ScriptableObject
    {
        #region Serialized Fields

        [Tooltip("Durée en secondes pendant laquelle une anomalie peut être reportée avant d'expirer.")]
        [SerializeField] private float _timeToReportAnomalies = 300f;

        [SerializeField, HideInInspector]
        private List<BuyoncyAnomalyDatas> _anomalies = new List<BuyoncyAnomalyDatas>();

        #endregion

        #region Events

        public event Action<ISignal> OnAnomalyAdded;
        public event Action<ISignal> OnAnomalyRemoved;
        public event Action<BuyoncyAnomalyDatas> OnAnomalyExpired;

        #endregion

        #region Properties

        public float TimeToReportAnomalies => _timeToReportAnomalies;

        #endregion

        #region Public API

        /// <summary>
        /// Ajoute ou réinitialise une anomalie pour une bouée donnée.
        /// </summary>
        public void SetAnomaly(int id)
        {
            var existing = _anomalies.Find(a => a.ID == id);
            if (existing != null)
            {
                existing.RemainingTime = _timeToReportAnomalies;
            }
            else
            {
                var data = new BuyoncyAnomalyDatas
                {
                    ID = id,
                    RemainingTime = _timeToReportAnomalies,
                    DisplayText = "Anomaly Detected"
                };

                _anomalies.Add(data);
                OnAnomalyAdded?.Invoke(data);
            }
        }

        /// <summary>
        /// Supprime une anomalie par son ID.
        /// </summary>
        public void RemoveAnomaly(int id)
        {
            var anomaly = _anomalies.Find(a => a.ID == id);
            if (anomaly != null)
            {
                _anomalies.Remove(anomaly);
                OnAnomalyRemoved?.Invoke(anomaly);
            }
        }

        /// <summary>
        /// Supprime une liste d'anomalies.
        /// </summary>
        public void RemoveAnomalies(List<int> ids)
        {
            var toRemove = _anomalies.Where(a => ids.Contains(a.ID)).ToList();

            foreach (var anomaly in toRemove)
            {
                _anomalies.Remove(anomaly);
                OnAnomalyRemoved?.Invoke(anomaly);
            }
        }

        /// <summary>
        /// Retourne toutes les anomalies en cours.
        /// </summary>
        public IReadOnlyList<BuyoncyAnomalyDatas> GetAnomalies() => _anomalies;

        /// <summary>
        /// Vérifie si une anomalie est enregistrée pour l'ID donné.
        /// </summary>
        public bool HasAnomaly(int id) => _anomalies.Exists(a => a.ID == id);

        [SerializeField] private bool _autoRemoveOnExpired = true;
        /// <summary>
        /// Met à jour le temps restant des anomalies et déclenche leur expiration si nécessaire.
        /// </summary>
        public void TickTimers(float deltaTime)
        {
            for (int i = 0; i < _anomalies.Count; i++)
                _anomalies[i].RemainingTime -= deltaTime;

            var expired = _anomalies.Where(a => a.RemainingTime <= 0f).ToList();
            foreach (var anomaly in expired)
            {
                OnAnomalyExpired?.Invoke(anomaly);

                if (_autoRemoveOnExpired)
                {
                    _anomalies.Remove(anomaly);
                    OnAnomalyRemoved?.Invoke(anomaly);
                }
            }
        }

        /// <summary>
        /// Supprime toutes les anomalies.
        /// </summary>
        public void ResetAnomalies()
        {
            _anomalies.Clear();
        }

        #endregion
    }
}
