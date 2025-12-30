using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch.Boats
{
    /// <summary>
    /// Calculs d'argent : plat + bonus basé sur une courbe/temps.
    /// </summary>
    public static class BoatMoneyCalculator
    {
        public static int ValidFlat(SO_BoatMoneyResults cfg) =>
            cfg.CorrectValidBoatReport;

        public static int MissmatchFlat(int count, SO_BoatMoneyResults cfg) =>
            count * cfg.AmountLostPerAttempt;

        /// <summary>
        /// Bonus croissant avec la vitesse (0s -> 0, MaxTime -> 1), puis courbe éventuellement inversée.
        /// Ici : on inverse la courbe décroissante pour obtenir une valeur croissante avec la vitesse.
        /// </summary>
        public static int BonusFromTime(
            float remainingTime,
            float maxTimeSeconds,
            SO_BoatMoneyResults cfg)
        {
            float t01 = Mathf.InverseLerp(0f, maxTimeSeconds, remainingTime);
            float curveValue = 1f - cfg.DecreaseCurveOverTime.Evaluate(t01);
            int money = Mathf.RoundToInt(curveValue * cfg.MaxMoneyOnOverTime);
            return money;
        }
    }

}

