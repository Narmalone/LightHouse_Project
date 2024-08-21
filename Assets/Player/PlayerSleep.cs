using System.Collections;
using UnityEngine;

public class PlayerSleep : MonoBehaviour
{
    [SerializeField] private CustomEvent _eventResetSleepAmount;
    [SerializeField] private CustomEvent_Float _eventOnUpdateSleep;
    [SerializeField] private float _deltaTimeToRemoveSleep;
    [SerializeField] private float _stepRemoveSleep;
    [SerializeField] private float _currentSleepAmount;

    private WaitForSeconds _waitForSleep;
    private Coroutine _coroutineHunger;

    public float CurrentSleepAmount
    {
        get { return _currentSleepAmount; }
        set
        {
            _currentSleepAmount = Mathf.Max(0, value);
            _eventOnUpdateSleep.Raise(_currentSleepAmount);
        }
    }

    private void Awake()
    {
        _waitForSleep = new WaitForSeconds(_deltaTimeToRemoveSleep);
        _eventResetSleepAmount.handle += OnResetSleep;
    }

    private void Start()
    {
        _coroutineHunger = StartCoroutine(Hunger_Coroutine());
    }

    private void OnDestroy()
    {
        _eventResetSleepAmount.handle -= OnResetSleep;
    }

    public void OnResetSleep()
    {
        CurrentSleepAmount = 100;
    }

    IEnumerator Hunger_Coroutine()
    {
        while (true)
        {
            CurrentSleepAmount -= _stepRemoveSleep;
            yield return _waitForSleep;
        }
    }
}