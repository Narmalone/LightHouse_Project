using TMPro;
using UnityEngine;

public class ChronometerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _chronometertext;

    private float _currentChronometerValue;
    private bool _isInitialized = false;

    public void Initialize()
    {
        _currentChronometerValue = 0f;
        _isInitialized = true;
    }

    public void Stop()
    {
        _isInitialized = false;
    }

    private void Update()
    {
        if (_isInitialized) UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (!_isInitialized) return;

        _currentChronometerValue += Time.deltaTime;
        _currentChronometerValue = Mathf.Round(_currentChronometerValue);

        if (_chronometertext == null) return;
        _chronometertext.text = _currentChronometerValue.ToString();
    }
}
