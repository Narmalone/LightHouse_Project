using LightHouse.Core.Player.Money;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace LightHouse.Features.Computer.LEO.Supplies
{
    /// <summary>
    /// Fenêtre de confirmation d'une commande de ravitaillement.
    /// Affiche l'argent, le coût total, le reste et l'ETA du prochain shipment.
    /// </summary>
    public class SupplyPopUp : MonoBehaviour
    {
        #region ===== Serialized UI References =====

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _currentMoneyValueTxt;
        [SerializeField] private TextMeshProUGUI _totalOrderValueTxt;
        [SerializeField] private TextMeshProUGUI _moneyLeftAfterOrderValueTxt;
        [SerializeField] private TextMeshProUGUI _deliveryScheduleValueTxt;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        #endregion

        #region ===== Public Accessors =====

        public Button ConfirmOrderButton => _confirmButton;
        public Button CancelOrderButton => _cancelButton;

        #endregion

        #region ===== Public API =====

        /// <summary>
        /// Initialise l'UI du popup.
        /// </summary>
        /// <param name="totalOrderAmount">Montant total du panier.</param>
        /// <param name="etaHours">
        /// Temps restant AVANT le prochain shipment, en HEURES de jeu.
        /// Exemple : 36 → “&gt; 2 days”, 12 → “12 hours”.
        /// </param>
        public void Initialize(float totalOrderAmount, float etaHours)
        {
            UpdateMoneyTexts(totalOrderAmount);
            UpdateEtaLabel(etaHours);
        }

        /// <summary>
        /// Si &lt; 24h → “H hour(s)”
        /// Si ≥ 24h → “&gt; D day(s)” (arrondi au supérieur)
        /// </summary>
        public static string FormatEta(float etaHours)
        {
            etaHours = Mathf.Max(0f, etaHours);

            if (etaHours >= 24f)
            {
                int days = Mathf.CeilToInt(etaHours / 24f); // arrondi au supérieur
                return $"> {days} {(days > 1 ? "days" : "day")}";
            }
            else
            {
                // Arrondi au supérieur pour éviter “0 hours” lorsqu'il reste <1h
                int hours = Mathf.Max(1, Mathf.CeilToInt(etaHours));
                return $"{hours} {(hours > 1 ? "hours" : "hour")}";
            }
        }

        #endregion

        #region ===== Private Helpers =====

        /// <summary>Mets à jour les trois champs d'argent.</summary>
        private void UpdateMoneyTexts(float totalOrderAmount)
        {
            float balance = PlayerCurrency.Balance;

            // SetText évite un peu d'alloc par rapport à string concat
            _currentMoneyValueTxt.text = $"{balance.ToString("N0")}$";
            _totalOrderValueTxt.text = $"- {totalOrderAmount.ToString("N0")}$";
            _moneyLeftAfterOrderValueTxt.text = $"{(balance - totalOrderAmount).ToString("N0")}$";
        }

        /// <summary>Mets à jour le libellé de l'ETA.</summary>
        private void UpdateEtaLabel(float etaHours)
        {
            string etaLabel = FormatEta(etaHours);
            _deliveryScheduleValueTxt.text = $"--- Next shipment in {etaLabel} ---";
        }

        #endregion
    }

}
