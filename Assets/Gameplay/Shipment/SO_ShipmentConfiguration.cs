using UnityEngine;


namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>
    /// Configuration générale des expéditions (délais, retard météo, heure de livraison).
    /// </summary>
    [CreateAssetMenu(fileName = "SO_ShipmentConfig", menuName = "LightHouse/Shipment/New Shipment Config")]
    public class SO_ShipmentConfiguration : ScriptableObject
    {
        #region ===== Serialized Fields (design-time) =====

        [Header("Lead Times (in-game hours)")]
        [Tooltip("Durée d'attente standard avant expédition (en HEURES de jeu). Ex: 48 = 2 jours in-game.")]
        [Range(0.5f, 150f)]
        public float ShipmentSchedule = 48f;

        [Tooltip("Retard additionnel appliqué si la météo est défavorable (en HEURES de jeu).")]
        [Range(0.5f, 150f)]
        public float ShipmentDelayTime = 24f;

        [Header("Dispatch Window (in-game hour)")]
        [Tooltip("Heure in-game de livraison/dispatch. 9.0 = 09:00. Doit être dans [0..24).")]
        [Range(0f, 23.9f)]
        public float ShipmentDeliveryHour = 9.0f;

        [Header("Shipment Order Config")]
        [Tooltip("Nombres d'objets maximum que le joueur peut ajouter dans le panier")]
        public int MaxItemsPerShipment = 30;

        #endregion

        #region ===== Convenience API (runtime helpers) =====

        /// <summary>
        /// Retourne le lead total (en HEURES de jeu) en tenant compte du retard météo.
        /// </summary>
        public float GetTotalLeadHours(bool shouldBeDelayed)
            => Mathf.Max(0.5f, ShipmentSchedule + (shouldBeDelayed ? ShipmentDelayTime : 0f));

        /// <summary>
        /// Convertit le lead total en SECONDES RÉELLES via la TimeConfiguration fournie.
        /// </summary>
        public float GetTotalLeadRealSeconds(TimeConfiguration cfg, bool shouldBeDelayed)
            => (cfg == null) ? 0f : cfg.GameHoursToRealSeconds(GetTotalLeadHours(shouldBeDelayed));

        #endregion

        #region ===== Validation (editor safety) =====

        private void OnValidate()
        {
            // Garantit des bornes sûres si les sliders sont modifiés via script ou import.
            ShipmentSchedule = Mathf.Clamp(ShipmentSchedule, 0.5f, 150f);
            ShipmentDelayTime = Mathf.Clamp(ShipmentDelayTime, 0.5f, 150f);
            ShipmentDeliveryHour = Mathf.Clamp(ShipmentDeliveryHour, 0f, 23.9f);
        }

        #endregion
    }
}

