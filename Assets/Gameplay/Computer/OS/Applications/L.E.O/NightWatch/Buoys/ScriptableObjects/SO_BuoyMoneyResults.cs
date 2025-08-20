using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch.Buoys
{
    [CreateAssetMenu(fileName = "Money_Buoy_Config_", menuName = "LightHouse/LEO/NightWatch/Buoys/New Money Config")]
    public class SO_BuoyMoneyResults : ScriptableObject
    {
        /// <summary>
        /// Montant le joueur gagne quand il donne la validitée correcte d'une bouée
        /// </summary>
        public int CorrectValidBuoyReport = 10;

        /// <summary>
        /// Montant que le joueur gagne quand il donne l'invaliditée correcte d'une bouée
        /// </summary>
        public int CorrectInvalidReport = 15;

        /// <summary>
        /// Montant que le joueur perds par bouée inccorecte
        /// </summary>
        public int AmountLostPerMissmatch = 5;

        /// <summary>
        /// Lorsqu'une bouée est invalide, plus le joueur signale vite la bouée qui est invalide plus il gagne d'argent
        /// </summary>
        public AnimationCurve DecreaseCurveOverTime = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Montant maximum si le joueur signale une bouée invalide et qu'il la signale presque instantanément
        /// Cette valeur diminue jusqu'à attendre 0 au bout de 5min par exemple.
        /// </summary>
        public int MaxMoneyOnOverTime = 20;

    }
}
