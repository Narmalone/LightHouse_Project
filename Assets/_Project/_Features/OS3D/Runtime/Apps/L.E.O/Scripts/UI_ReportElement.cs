using TMPro;
using UnityEngine;

namespace LightHouse.Features.Computer.LEO
{
    public class UI_ReportElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _reasonTxt;
        [SerializeField] private TextMeshProUGUI _moneyResultTxt;

        public TextMeshProUGUI ReasonText => _reasonTxt;
        public TextMeshProUGUI MoneyResult => _moneyResultTxt;

        public void SetDescription(string reason)
        {
            _reasonTxt.text = reason;
        }

        public void SetMoneyResult(string moneyResult, Color textColor)
        {
            _moneyResultTxt.text = moneyResult;
            _moneyResultTxt.color = textColor;
        }
    }

}
