using UnityEngine;

namespace LightHouse.Features.Computer.LEO.NightWatch.Boats
{
    [CreateAssetMenu(fileName = "Money_Boat_Config_", menuName = "LightHouse/LEO/NightWatch/Boats/New Money Config")]
    public class SO_BoatMoneyResults : ScriptableObject
    {
        /// <summary>
        /// Montant le joueur gagne quand il donne la validitée correcte d'un bateau
        /// </summary>
        public int CorrectValidBoatReport = 25;

        /// <summary>
        /// Montant que le joueur perds par bouée inccorecte
        /// </summary>
        public int AmountLostPerAttempt = 10;

        /// <summary>
        /// Lorsqu'une bouée est invalide, plus le joueur signale vite la bouée qui est invalide plus il gagne d'argent
        /// </summary>
        public AnimationCurve DecreaseCurveOverTime = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Montant maximum si le joueur signale une bouée invalide et qu'il la signale presque instantanément
        /// Cette valeur diminue jusqu'à attendre 0 au bout de 5min par exemple.
        /// </summary>
        public int MaxMoneyOnOverTime = 35;
    }
}
