using TMPro;
using UnityEngine;

public class ChronometerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _chronometertext;

    private float _currentChronometerValue;
    private bool _isInitialized = false;

    public void StartChrono()
    {
        if(_currentChronometerValue > 0f)
        {
            _currentChronometerValue = 0f;
            UpdateChronometerText();
        }
        _isInitialized = true;
    }

    public void StopChrono()
    {
        _currentChronometerValue = 0f;
        _isInitialized = false;
        UpdateChronometerText();
    }

    private void Update()
    {
        if (_isInitialized) UpdateTimer();
    }

    private void UpdateTimer()
    {
        _currentChronometerValue += Time.deltaTime;

        int seconds = Mathf.FloorToInt(_currentChronometerValue);

        if (_chronometertext == null) return;
        UpdateChronometerText();
    }

    private void UpdateChronometerText()
    {
        int seconds = Mathf.FloorToInt(_currentChronometerValue);
        _chronometertext.text = seconds.ToString("000");
    }
}
