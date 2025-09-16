using LightHouse.Money;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupplyPopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentMoneyValueTxt;
    [SerializeField] private TextMeshProUGUI _totalOrderValueTxt;
    [SerializeField] private TextMeshProUGUI _moneyLeftAfterOrderValueTxt;
    [SerializeField] private TextMeshProUGUI _deliveryScheduleValueTxt;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    public Button ConfirmOrderButton => _confirmButton;
    public Button CancelOrderButton => _cancelButton;

    /// <param name="etaHours">
    /// Temps restant AVANT le prochain shipment, en **heures de jeu**.
    /// Exemple : 36 → “> 2 days”, 12 → “12 hours”.
    /// </param>
    public void Initialize(float totalOrderAmount, float etaHours)
    {
        // Argent
        _currentMoneyValueTxt.text = PlayerCurrency.Balance.ToString("N0") + "$";
        _totalOrderValueTxt.text = "- " + totalOrderAmount.ToString("N0") + "$";
        _moneyLeftAfterOrderValueTxt.text = (PlayerCurrency.Balance - totalOrderAmount).ToString("N0") + "$";

        // ETA formaté (heures si < 24h, sinon > X days)
        string etaLabel = FormatEta(etaHours);
        Debug.Log(etaHours);
        _deliveryScheduleValueTxt.text = $"--- Next shipment in {etaLabel} ---";
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
            // Arrondi au supérieur pour éviter “0 hours” quand il reste un peu de temps
            int hours = Mathf.Max(1, Mathf.CeilToInt(etaHours));
            return $"{hours} {(hours > 1 ? "hours" : "hour")}";
        }
    }
}
