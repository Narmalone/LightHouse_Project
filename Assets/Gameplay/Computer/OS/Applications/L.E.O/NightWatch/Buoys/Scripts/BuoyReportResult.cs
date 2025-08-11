using System.Collections.Generic;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{
    /// <summary>
    /// Contient le résultat d'une évaluation des rapports de bouées.
    /// Utilisé pour transmettre les statistiques et les données nécessaires
    /// à la mise à jour de l'état du jeu et de l'interface.
    /// </summary>
    public sealed class BuoyReportResult
    {
        #region Public Properties

        /// <summary>
        /// Nombre de bouées valides correctement identifiées.
        /// </summary>
        public int CorrectValidCount { get; }

        /// <summary>
        /// Nombre de bouées invalides correctement identifiées.
        /// </summary>
        public int CorrectInvalidCount { get; }

        /// <summary>
        /// Nombre total d'erreurs (mauvaise identification).
        /// </summary>
        public int ErrorCount { get; }

        /// <summary>
        /// Liste des temps restants pour les bouées invalides correctement identifiées.
        /// Utilisé pour calculer les bonus basés sur la vitesse de rapport.
        /// </summary>
        public IReadOnlyList<float> RemainingTimesForCorrectInvalid { get; }

        /// <summary>
        /// Liste des IDs d'anomalies à retirer de la base après évaluation.
        /// </summary>
        public IReadOnlyList<int> AnomalyIdsToRemove { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Crée un nouveau résultat d'évaluation des rapports de bouées.
        /// </summary>
        /// <param name="correctValidCount">Nombre de bouées valides correctement identifiées.</param>
        /// <param name="correctInvalidCount">Nombre de bouées invalides correctement identifiées.</param>
        /// <param name="errorCount">Nombre total d'erreurs.</param>
        /// <param name="remainingTimesForCorrectInvalid">Temps restants pour les bouées invalides correctement identifiées.</param>
        /// <param name="anomalyIdsToRemove">IDs d'anomalies à retirer de la base.</param>
        public BuoyReportResult(
            int correctValidCount,
            int correctInvalidCount,
            int errorCount,
            IReadOnlyList<float> remainingTimesForCorrectInvalid,
            IReadOnlyList<int> anomalyIdsToRemove)
        {
            CorrectValidCount = correctValidCount;
            CorrectInvalidCount = correctInvalidCount;
            ErrorCount = errorCount;
            RemainingTimesForCorrectInvalid = remainingTimesForCorrectInvalid;
            AnomalyIdsToRemove = anomalyIdsToRemove;
        }

        #endregion
    }
}
