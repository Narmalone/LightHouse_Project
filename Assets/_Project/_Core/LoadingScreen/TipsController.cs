using LightHouse.Core.Inputs;
using LightHouse.Core.Utilities;
using LightHouse.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TipsController : MonoBehaviour
{
    [SerializeField] private float _timerTipsDuration = 7.5f;
    [SerializeField] private LocalizedStringDatabase_Tips _tips;
    [SerializeField] private TextMeshProUGUI _tipsText;

    private Timer _timer;
    private int _currentTipIndex = 0;

    private void Awake()
    {
        _timer = new Timer(_timerTipsDuration);
        _tipsText.text = "";
        _timer.OnTimerComplete += Timer_OnTimerComplete;
        _timer.StartTimer();

    }

    private void OnEnable()
    {
        InputManager.UI.Click.performed += Click_performed;
    }

    private void OnDisable()
    {
        InputManager.UI.Click.performed -= Click_performed;
    }

    private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!gameObject.activeInHierarchy) return;
        ShowNextTip();
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        _timer?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (_timer != null)
            _timer.OnTimerComplete -= Timer_OnTimerComplete;
    }

    private void Timer_OnTimerComplete()
    {
        ShowNextTip();
    }

    public void ShowTip(int index)
    {
        if (_tips == null || _tips.All == null || _tips.All.Length == 0)
        {
            Debug.LogWarning("❌ Aucun tip disponible !");
            return;
        }

        // Clamp pour éviter crash
        index = Mathf.Clamp(index, 0, _tips.All.Length - 1);
        _currentTipIndex = index;

        var handle = _tips.All[index].GetLocalizedStringAsync();

        handle.Completed += (AsyncOperationHandle<string> op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _tipsText.text = op.Result;
            }
        };

        _timer.ResetTimer();
    }

    public void ShowNextTip()
    {
        if (_tips == null || _tips.All == null || _tips.All.Length == 0)
            return;

        _currentTipIndex++;

        if (_currentTipIndex >= _tips.All.Length)
        {
            _currentTipIndex = 0;
        }

        ShowTip(_currentTipIndex);
    }

    public void ShowRandomTip()
    {
        _currentTipIndex = Random.Range(0, _tips.All.Length);
        ShowTip(_currentTipIndex);
    }
}