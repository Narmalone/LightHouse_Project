using LightHouse.Game.Computer.OS;
using System.Collections.Generic;
using UnityEngine;

public enum ELEOWindow
{
    Menu,
    NightWatch,
    Meteo
}


public class LEOApplication : ComputerApp
{
    [SerializeField] private LEOWindow[] _windows;

    private Dictionary<ELEOWindow, LEOWindow> _windowMap;
    public LEOWindow CurrentActiveWindow { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        _windowMap = new Dictionary<ELEOWindow, LEOWindow>();

        foreach (var window in _windows)
        {
            window.Close();
            if (!_windowMap.ContainsKey(window.Type))
                _windowMap.Add(window.Type, window);
            else
                Debug.LogWarning($"Duplicate window type: {window.Type}");
        }
    }

    private void Start()
    {
        if (isActiveAndEnabled) State = E_ComputerAppState.Opened;
        ShowWindow(ELEOWindow.Menu);
    }

    public void ShowWindow(ELEOWindow type)
    {
        if (!_windowMap.TryGetValue(type, out var newWindow))
        {
            Debug.LogWarning($"Window type {type} not found.");
            return;
        }

        if (newWindow == CurrentActiveWindow)
            return;

        CurrentActiveWindow?.Close();
        CurrentActiveWindow = newWindow;
        CurrentActiveWindow.Open();
    }

    public override void OnOpen() { }
    public override void OnClose() { }
    public override void OnMinimize() { }
}
