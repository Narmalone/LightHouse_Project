using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{
    /// <summary>
    /// Calculs d'argent : plat + bonus basé sur une courbe/temps.
    /// </summary>
    public static class MoneyCalculator
    {
        public static int ValidFlat(int count, SO_BuoyMoneyResults cfg) =>
            count * cfg.CorrectValidBuoyReport;

        public static int InvalidFlat(int count, SO_BuoyMoneyResults cfg) =>
            count * cfg.CorrectInvalidReport;

        public static int MissmatchFlat(int count, SO_BuoyMoneyResults cfg) =>
            count * cfg.AmountLostPerMissmatch;

        /// <summary>
        /// Bonus croissant avec la vitesse (0s -> 0, MaxTime -> 1), puis courbe éventuellement inversée.
        /// Ici : on inverse la courbe décroissante pour obtenir une valeur croissante avec la vitesse.
        /// </summary>
        public static int BonusFromTimes(
            IEnumerable<float> remainingTimes,
            float maxTimeSeconds,
            SO_BuoyMoneyResults cfg)
        {
            int total = 0;

            foreach (var seconds in remainingTimes)
            {
                float t01 = Mathf.InverseLerp(0f, maxTimeSeconds, seconds);
                float curveValue = 1f - cfg.DecreaseCurveOverTime.Evaluate(t01);
                int money = Mathf.RoundToInt(curveValue * cfg.MaxMoneyOnOverTime);
                total += money;
            }

            return total;
        }
    }
}

