using System;

public class Timer
{
    public float _timerTotalDuration;
    private float _timerDuration; // elapsed time
    private bool _isRunning; 
    public bool IsRunning => _isRunning;

    public Timer(float timerDuration)
    {
        _timerTotalDuration = timerDuration;
    }

    public event Action OnTimerStart;
    public event Action<float> OnTimerTick; 
    public event Action OnTimerStop; 
    public event Action OnTimerReset;
    public event Action OnTimerComplete;

    public void Tick(float deltaTime, float timerSpeed = 1.0f)
    {
        if (_isRunning)
        {
            _timerDuration += deltaTime * timerSpeed;
            OnTimerTick?.Invoke(_timerDuration);
            if (_timerDuration >= _timerTotalDuration)
            {
                _isRunning = false;
                _timerDuration = _timerTotalDuration;
                OnTimerComplete?.Invoke(); 
            }
        }
    }

    public void StartTimer()
    {
        _timerDuration = 0f;
        _isRunning = true;
        OnTimerStart?.Invoke();
    }

    public void StopTimer()
    {
        _isRunning = false;
        OnTimerStop?.Invoke();
    }

    public void ResetTimer()
    {
        _timerDuration = 0f;
        _isRunning = false;
        OnTimerReset?.Invoke();
    }

    public void ResetTimer(float newTotalDuration)
    {
        _timerTotalDuration = newTotalDuration;
        _timerDuration = 0f;
        _isRunning = false;
        OnTimerReset?.Invoke();
    }


    public float GetTimeRemaining()
    {
        return _timerTotalDuration - _timerDuration;
    }
}