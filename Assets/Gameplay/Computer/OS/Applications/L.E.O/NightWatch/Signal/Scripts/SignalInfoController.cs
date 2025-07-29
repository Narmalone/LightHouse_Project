using TMPro;
using UnityEngine;

public class SignalInfoController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _signalTypeText;
    [SerializeField] private TextMeshProUGUI _signalTimeDateText;
    [SerializeField] private TextMeshProUGUI _signalFailureTimerText;

    public TextMeshProUGUI SignalText => _signalTypeText;
    public TextMeshProUGUI TimeDateText => _signalTimeDateText;
    public TextMeshProUGUI FailureTimerText => _signalFailureTimerText;

    public void UpdateInformations(string signalType, string arrivalDate, string failureTimer)
    {
        _signalTypeText.text = signalType;
        _signalTimeDateText.text = arrivalDate;
        _signalFailureTimerText.text = failureTimer;
    }

    public void UpdateTimer()
    {

    }
}
