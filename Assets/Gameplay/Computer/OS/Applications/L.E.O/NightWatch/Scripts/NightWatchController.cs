using LightHouse.Game.Computer.OS;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum E_NightWatchMode
{
    Boat,
    Buoys
}

public class NightWatchController : LEOWindow
{
    [SerializeField] private LEOApplication _manager;
    [SerializeField] private TabCanvas _tabCanvas;
    [SerializeField] private NightWatchReportWindow[] _windows;
    [SerializeField] private SonarUI _sonarUIController;
    [SerializeField] private UI_BuoysReportController _buoysUIController;

    private Dictionary<E_NightWatchMode, NightWatchReportWindow> _windowMap;
    private NightWatchReportWindow _activeWindow;

    private void Awake()
    {
        _windowMap = new Dictionary<E_NightWatchMode, NightWatchReportWindow>();
        foreach (var w in _windows)
        {
            w.Close();
            if (!_windowMap.ContainsKey(w.WindowType))
                _windowMap.Add(w.WindowType, w);
            else
                Debug.LogWarning($"Duplicate NightWatch window type: {w.WindowType}");
        }
    }

    private void OnEnable()
    {
        if(_manager.State == E_ComputerAppState.Opened && _manager.OS.PlayerOnComputer)
        {
            _sonarUIController.StartRadar();
        }
    }
    private void Start()
    {
        SwitchTo(E_NightWatchMode.Boat);
    }

    private void OnDisable()
    {
        _sonarUIController.StopRadar();
    }

    public void SwitchTo(E_NightWatchMode target)
    {
        if (_windowMap.TryGetValue(target, out NightWatchReportWindow newWindow))
        {
            if (newWindow == _activeWindow) return;

            _activeWindow?.Close();
            _activeWindow = newWindow;
            _activeWindow.Open();
        }
        else
        {
            Debug.LogWarning($"No NightWatch window for type {target}");
        }
    }

}
