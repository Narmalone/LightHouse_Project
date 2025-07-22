using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour, IDisplayable
{
    [SerializeField] private Timer _timer;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _timerValue = 10f;

    private string _timeLeft;

    private void Start()
    {
        _timer._timerTotalDuration = _timerValue;
    }

    private void Update()
    {
        SetTimerText();
    }

    void SetTimerText()
    {
        _timerText.text = _timeLeft;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _timer.StartTimer();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _timer.StopTimer();
    }
}
