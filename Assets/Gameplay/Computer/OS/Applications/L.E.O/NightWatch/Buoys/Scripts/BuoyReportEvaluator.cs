using System.Collections.Generic;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{
    /// <summary>
    /// Évalue un rapport de bouées en fonction des anomalies en cours.
    /// Cette classe ne gère que la logique métier (aucun effet sur l'UI ou sur la base).
    /// </summary>
    public sealed class BuoyReportEvaluator
    {
        #region Public Methods

        /// <summary>
        /// Analyse la liste des bouées et calcule le résultat d'évaluation.
        /// </summary>
        /// <param name="buoys">
        /// Collection de bouées à évaluer.
        /// Chaque bouée possède un état actuel et une indication si elle a déjà été reportée aujourd'hui.
        /// </param>
        /// <param name="anomalyRemainingTimes">
        /// Dictionnaire des anomalies en cours :
        /// - Clé = ID de la bouée avec anomalie
        /// - Valeur = Temps restant avant expiration (en secondes).
        /// </param>
        /// <returns>
        /// Un <see cref="BuoyReportResult"/> contenant le nombre de bonnes réponses, erreurs,
        /// temps restants pour bonus, et les IDs d'anomalies à retirer.
        /// </returns>
        public BuoyReportResult Evaluate(
            IEnumerable<UI_Buoy> buoys,
            IReadOnlyDictionary<int, float> anomalyRemainingTimes)
        {
            int correctValid = 0;
            int correctInvalid = 0;
            int errors = 0;

            var timesForBonus = new List<float>();
            var idsToRemove = new List<int>();

            foreach (var buoy in buoys)
            {
                if (ShouldSkipEvaluation(buoy))
                    continue;

                bool shouldBeInvalid = anomalyRemainingTimes.ContainsKey(buoy.ID);
                bool isMarkedInvalid = buoy.CurrentState == UI_BuoyState.Invalid;
                bool isMarkedValid = buoy.CurrentState == UI_BuoyState.Valid;

                if (shouldBeInvalid && isMarkedInvalid)
                {
                    ProcessCorrectInvalid(buoy, anomalyRemainingTimes, timesForBonus, idsToRemove);
                    correctInvalid++;
                }
                else if (!shouldBeInvalid && isMarkedValid)
                {
                    correctValid++;
                }
                else
                {
                    errors++;
                }
            }

            return new BuoyReportResult(
                correctValid,
                correctInvalid,
                errors,
                timesForBonus,
                idsToRemove
            );
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Détermine si une bouée doit être ignorée lors de l'évaluation.
        /// </summary>
        private static bool ShouldSkipEvaluation(UI_Buoy buoy) =>
            buoy.CurrentState == UI_BuoyState.Unchecked || buoy.HasBeenReportedToday;

        /// <summary>
        /// Traite une bouée invalid correctement signalée.
        /// </summary>
        private static void ProcessCorrectInvalid(
            UI_Buoy buoy,
            IReadOnlyDictionary<int, float> anomalyRemainingTimes,
            List<float> timesForBonus,
            List<int> idsToRemove)
        {
            timesForBonus.Add(anomalyRemainingTimes[buoy.ID]);
            idsToRemove.Add(buoy.ID);
        }

        #endregion
    }
}
