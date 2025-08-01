using LightHouse.Game.Computer.OS;
using System.Collections.Generic;
using UnityEngine;

public enum ELEOWindow
{
    Menu,
    NightWatch,
    CameraView,
    Meteo
}


public class LEOApplication : ComputerApp
{
    [SerializeField] private TabCanvas _tabCanvas;
    [SerializeField] private LEOWindow[] _windows;
    [SerializeField] private LEOWindowButton[] _windowsButton;
    [SerializeField] private NightWatchController _nightWatchController;

    private Dictionary<ELEOWindow, LEOWindow> _windowMap;
    public LEOWindow CurrentActiveWindow { get; private set; }
    public NightWatchController NightWatch => _nightWatchController;

    private bool _firstTimeOpening = false;

    protected override void Awake()
    {
        base.Awake();
        _windowMap = new Dictionary<ELEOWindow, LEOWindow>();
      
    }

    public override void Initialize(OS os)
    {
        base.Initialize(os);
        foreach (var window in _windows)
        {
            window.OSSystem = this._os;
            window.Close();
            if (!_windowMap.ContainsKey(window.Type))
                _windowMap.Add(window.Type, window);
            else
                Debug.LogWarning($"Duplicate window type: {window.Type}");
        }
    }

    private void OnValidate()
    {
        _windows = GetComponentsInChildren<LEOWindow>();
        _windowsButton = GetComponentsInChildren<LEOWindowButton>();


        foreach (var windowButton in _windowsButton)
        {
            windowButton.App = this;
        }
    }

    public void ShowWindow(ELEOWindow type)
    {
        if (!_windowMap.TryGetValue(type, out var newWindow))
        {
            Debug.LogWarning($"Window type {type} not found.");
            return;
        }
        if (isActiveAndEnabled) State = E_ComputerAppState.Opened;

        if (newWindow == CurrentActiveWindow)
            return;

        CurrentActiveWindow?.Close();
        CurrentActiveWindow = newWindow;
        CurrentActiveWindow.Open();
    }

    public override void OnOpen() 
    {
        _tabCanvas.EnableCanvasGroup();

        if (!_firstTimeOpening)
        {
            ShowWindow(ELEOWindow.Menu);
            _firstTimeOpening = true;
        }
    }
    public override void OnClose() 
    {
        _tabCanvas.DisableCanvasGroup();
    }
    public override void OnMinimize() { }
}
