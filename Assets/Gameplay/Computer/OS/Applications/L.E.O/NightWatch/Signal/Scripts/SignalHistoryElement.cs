using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
