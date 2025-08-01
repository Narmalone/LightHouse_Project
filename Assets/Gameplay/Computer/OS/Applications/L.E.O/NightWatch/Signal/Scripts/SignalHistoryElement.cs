using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignalHistoryElement : MonoBehaviour
{
    [SerializeField] private Image Icon;
    [SerializeField] private TextMeshProUGUI _arrivalDateText;
    [SerializeField] private Image _completionState;

    public void SetInfos(Sprite icon, string arrivalDate, Sprite completionValidation)
    {
        Icon.sprite = icon;
        _arrivalDateText.text = arrivalDate;
        _completionState.sprite = completionValidation;
    }
}
