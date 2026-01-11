using LightHouse.Core.Player.Money;
using TMPro;
using UnityEngine;

namespace LightHouse.Core.Player.Money
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UI_PlayersMoney : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moneyValueText;

        private void Awake()
        {
            if (_moneyValueText == null)
                _moneyValueText = GetComponent<TextMeshProUGUI>();
            PlayerCurrency.OnBalanceChanged += PlayerCurrency_OnBalanceChanged;
        }

        private void Start()
        {
            UpdateUI(PlayerCurrency.Balance);
        }

        private void PlayerCurrency_OnBalanceChanged(float obj)
        {
            UpdateUI(obj);
        }

        private void UpdateUI(float value)
        {
            _moneyValueText.text = value.ToString();
        }

        private void OnDestroy()
        {
            PlayerCurrency.OnBalanceChanged -= PlayerCurrency_OnBalanceChanged;
        }
    }
}