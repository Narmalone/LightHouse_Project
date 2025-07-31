using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignalInfoController : MonoBehaviour
{
    [SerializeField] private Image _signalTypeIcon;
    [SerializeField] private TextMeshProUGUI _signalTimeDateText;
    [SerializeField] private TextMeshProUGUI _signalFailureTimerText;

    public Image SignalText => _signalTypeIcon;
    public TextMeshProUGUI TimeDateText => _signalTimeDateText;
    public TextMeshProUGUI FailureTimerText => _signalFailureTimerText;

    public float Timer;
    private float _endTime;

    public void StartTimer(float duration)
    {
        Timer = duration;
        _endTime = Time.realtimeSinceStartup + 60 * duration;
    }

    public void UpdateTimer()
    {
        Timer = _endTime - Time.realtimeSinceStartup;
        if (Timer < 0f) Timer = 0f;

        int minutes = Mathf.FloorToInt(Timer / 60f);
        int seconds = Mathf.FloorToInt(Timer % 60f);
        FailureTimerText.text = $"{minutes:00}:{seconds:00}";
    }


    public void UpdateInformations(Sprite signalIcon, string arrivalDate, string failureTimer)
    {
        _signalTypeIcon.sprite = signalIcon;
        _signalTimeDateText.text = arrivalDate;
        _signalFailureTimerText.text = failureTimer;
    }

    public void Update()
    {
        UpdateTimer();

        if(Timer <= 0f)
        {
            //Stop the timer and send an event
            //Generate it in the history
            //Having a fail
        }
    }
}
