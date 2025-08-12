using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Signals
{
    /// <summary>
    /// Signal affiché dans l'historique si il est réussi ou non
    /// le type de signal
    /// et information complémentaire comme date d'arrivée
    /// </summary>
    public class SignalHistoryElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _arrivalDateText;
        [SerializeField] private Image _completionState;

        public Image Icon => _icon;

        public void SetInfos(Sprite icon, string arrivalDate, Sprite completionValidation)
        {
            _icon.sprite = icon;
            _arrivalDateText.text = arrivalDate;
            _completionState.sprite = completionValidation;
        }
    }

}
