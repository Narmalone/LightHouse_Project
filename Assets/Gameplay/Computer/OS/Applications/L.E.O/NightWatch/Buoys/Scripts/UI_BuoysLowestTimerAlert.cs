using LightHouse.Game.Buyoncies;
using TMPro;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{
    /// <summary>
    /// Affiche en permanence le timer restant pour l'anomalie de bouée
    /// la plus urgente (temps restant le plus bas).
    /// </summary>
    public class UI_BuoysLowestTimerAlert : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private BuyoncyAnomalyDatabase _database;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _timerText;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            UpdateTimerDisplay(0f); // Valeur par défaut
        }

        private void Update()
        {
            // 1) Mise à jour interne de la base (décrémentation timers)
            _database.TickTimers(Time.deltaTime);

            // 2) Récupération de l'anomalie la plus urgente
            var anomalies = _database.GetAnomalies();
            if (anomalies.Count == 0)
            {
                UpdateTimerDisplay(0f);
                return;
            }

            var lowest = GetLowestRemainingTime(anomalies);
            UpdateTimerDisplay(lowest);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retourne le temps restant le plus bas parmi les anomalies données.
        /// </summary>
        private float GetLowestRemainingTime(System.Collections.Generic.IReadOnlyList<BuyoncyAnomalyDatas> anomalies)
        {
            float lowest = float.MaxValue;

            foreach (var anomaly in anomalies)
            {
                if (anomaly.RemainingTime < lowest)
                    lowest = anomaly.RemainingTime;
            }

            return lowest;
        }

        /// <summary>
        /// Met à jour l'affichage du timer au format MM:SS.
        /// </summary>
        private void UpdateTimerDisplay(float remainingTime)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        #endregion
    }
}
