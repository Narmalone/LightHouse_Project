using LightHouse.Features.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightHouse.Features.Boats.Anomalies
{
    #region Data Model

    /// <summary>
    /// Données d'une anomalie bateau, exposées comme <see cref="ISignal"/> pour l'UI / signaux.
    /// </summary>
    [Serializable]
    public class BoatAnomalyDatas : ISignal
    {
        [Tooltip("Nom unique du bateau (sert de clé).")]
        public string BoatName;

        [Tooltip("Fréquence unique du bateau")]
        public float BoatFrequency;

        [Tooltip("Type fonctionnel de l'anomalie.")]
        public AnomalyType AnomalyType;

        /// <summary>Temps restant avant expiration (secondes).</summary>
        public float RemainingTime { get; set; }

        public int TryAmount { get; set; }

        /// <summary>Texte d'affichage (ex: radio, infos diverses).</summary>
        public string DisplayText { get; set; }

        /// <inheritdoc/>
        public string Key => BoatFrequency.ToString();
    }

    #endregion

    /// <summary>
    /// Base d'anomalies des bateaux : ajout, suppression, mise à jour du timer, expiration.
    /// </summary>
    [CreateAssetMenu(fileName = "BoatAnomaliesDatabase", menuName = "LightHouse/Boats/New Anomaly Database")]
    public class BoatAnomaliesDatabase : ScriptableObject
    {
        #region Serialized Fields

        [Header("Timing")]
        [Tooltip("Durée maxi (en s) pendant laquelle une anomalie peut être signalée avant d'expirer.")]
        [Min(0f)]
        [SerializeField] private float _timeToReportAnomalies = 300f;

        [Tooltip("Si activé, une anomalie expirée est retirée automatiquement et l'événement 'Removed' est émis après 'Expired'.")]
        [SerializeField] private bool _autoRemoveOnExpire = false;

        [Header("Storage")]
        [SerializeField]
        private List<BoatAnomalyDatas> _anomalies = new();

        #endregion

        #region Events

        /// <summary>Émis lorsqu'une anomalie est ajoutée (ou upsert initial).</summary>
        public event Action<ISignal> OnAnomalyAdded;

        /// <summary>Émis lorsqu'une anomalie est retirée manuellement ou suite à expiration (si auto-remove).</summary>
        public event Action<ISignal> OnAnomalyRemoved;

        /// <summary>Émis lorsqu'une anomalie arrive à expiration (timer ≤ 0).</summary>
        public event Action<BoatAnomalyDatas> OnAnomalyExpired;

        #endregion

        #region Public API

        /// <summary>
        /// Crée ou réinitialise une anomalie pour le bateau.
        /// </summary>
        /// <param name="boatName">Nom unique du bateau.</param>
        /// <param name="anomalyType">Type d'anomalie.</param>
        /// <param name="displayText">Texte d'affichage (ex: "159.2 MHz").</param>
        public void SetAnomaly(string boatName, float frequency, AnomalyType anomalyType, string displayText)
        {
            if (string.IsNullOrWhiteSpace(boatName))
            {
                Debug.LogWarning("[BoatAnomaliesDatabase] SetAnomaly ignoré : boatName vide.");
                return;
            }

            var existing = _anomalies.Find(a => a.BoatName == boatName);
            if (existing != null)
            {
                existing.AnomalyType = anomalyType;
                existing.RemainingTime = _timeToReportAnomalies;
                existing.BoatFrequency = frequency;
                existing.DisplayText = displayText;
                existing.TryAmount = 0;
                // Pas d'event "updated" pour garder l'API simple.
            }
            else
            {
                var data = new BoatAnomalyDatas
                {
                    BoatName = boatName,
                    BoatFrequency = frequency,
                    AnomalyType = anomalyType,
                    RemainingTime = _timeToReportAnomalies,
                    DisplayText = displayText,
                    TryAmount = 0
                };
                _anomalies.Add(data);
                OnAnomalyAdded?.Invoke(data);
            }
        }

        /// <summary>
        /// Retire une anomalie via le nom de bateau.
        /// </summary>
        public void RemoveAnomaly(string boatName)
        {
            if (!TryGetAnomaly(boatName, out var anomaly)) return;

            _anomalies.Remove(anomaly);
            OnAnomalyRemoved?.Invoke(anomaly);
        }

        /// <summary>
        /// Essaie de récupérer l'anomalie d'un bateau.
        /// </summary>
        public bool TryGetAnomaly(string boatName, out BoatAnomalyDatas anomaly)
        {
            anomaly = _anomalies.Find(a => a.BoatName == boatName);
            return anomaly != null;
        }

        public bool TryGetAnomalyByFrequency(string frequency, out BoatAnomalyDatas anomaly)
        {
            anomaly = _anomalies.Find(a => a.Key == frequency);
            return anomaly != null;
        }

        public bool TryGetAnomaly(float freq, out BoatAnomalyDatas data, float epsilon = 0.05f)
        {
            foreach (var d in _anomalies)
            {
                if (Mathf.Abs(d.BoatFrequency - freq) <= epsilon)
                {
                    data = d;
                    return true;
                }
            }
            data = null;
            return false;
        }


        /// <summary>
        /// Essaie de récupérer l'anomaly d'un bateau par fréquences
        /// </summary>
        /// <returns></returns>
        public bool TryGetAnomaly(float frequency, out BoatAnomalyDatas anomaly)
        {
            anomaly = _anomalies.Find(a => a.BoatFrequency == frequency);
            return anomaly != null;
        }

        public bool TryGetAnomaly(string key, out BoatAnomalyDatas anomaly, bool ok = false)
        {
            anomaly = _anomalies.Find(a => a.Key == key);
            return anomaly != null;
        }

        /// <summary>
        /// Retourne la liste en lecture seule des anomalies en cours.
        /// </summary>
        public IReadOnlyList<BoatAnomalyDatas> GetAnomalies() => _anomalies;

        /// <summary>
        /// Indique si le bateau possède l'anomalie attendue.
        /// </summary>
        public bool HasAnomaly(string boatName, AnomalyType expectedAnomaly) =>
            _anomalies.Exists(a => a.BoatName == boatName && a.AnomalyType == expectedAnomaly);

        /// <summary>
        /// Indique si le bateau possède une anomalie (peu importe le type).
        /// </summary>
        public bool HasAnomaly(string boatName) =>
            _anomalies.Exists(a => a.BoatName == boatName);

        /// <summary>
        /// Vide complètement la base.
        /// </summary>
        public void ResetAnomalies() => _anomalies.Clear();

        #endregion

        #region Timer / Expiration

        /// <summary>
        /// À appeler chaque frame par un contrôleur : décrémente les timers et gère les expirations.
        /// </summary>
        public void TickTimers(float deltaTime)
        {
            if (_anomalies.Count == 0) return;

            for (int i = 0; i < _anomalies.Count; i++)
                _anomalies[i].RemainingTime -= deltaTime;

            // Snapshot pour éviter de modifier la collection pendant l'énumération
            var expired = _anomalies.Where(a => a.RemainingTime <= 0f).ToList();
            if (expired.Count == 0) return;

            foreach (var a in expired)
            {
                OnAnomalyExpired?.Invoke(a);

                if (_autoRemoveOnExpire)
                {
                    _anomalies.Remove(a);
                    OnAnomalyRemoved?.Invoke(a);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Accès (lecture seule) à la durée max d'un report.
        /// </summary>
        public float TimeToReportAnomalies => _timeToReportAnomalies;

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (_timeToReportAnomalies < 0f) _timeToReportAnomalies = 0f;
            if (_anomalies == null) _anomalies = new List<BoatAnomalyDatas>();
        }

        #endregion
    }
}
