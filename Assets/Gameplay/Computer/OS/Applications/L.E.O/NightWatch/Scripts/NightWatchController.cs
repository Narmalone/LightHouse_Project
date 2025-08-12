using LightHouse.Game.Computer.LEO.NightWatch.Buoys;
using LightHouse.Game.Computer.LEO.NightWatch.Signals;
using LightHouse.Game.Computer.LEO.NightWatch.Sonar;
using LightHouse.Game.Computer.OS;
using System.Collections.Generic;
using UnityEngine;


namespace LightHouse.Game.Computer.LEO.NightWatch
{
    public enum E_NightWatchMode
    {
        Boat,
        Buoys,
        Signals
    }

    public class NightWatchController : LEOWindow
    {
        [SerializeField] private LEOApplication _manager;
        [SerializeField] private TabCanvas _tabCanvas;
        [SerializeField] private LEOWindowButton _backButton;
        [SerializeField] private LEOWindowButton _switchToCamera;
        [SerializeField] private NightWatchReportWindow[] _windows;
        [SerializeField] private UI_Sonar _sonarUIController;
        [SerializeField] private UI_BuoysReportController _buoysReportController;
        [SerializeField] private UI_Signals _signalsController;

        private Dictionary<E_NightWatchMode, NightWatchReportWindow> _windowMap;
        private NightWatchReportWindow _activeWindow;

        private void Awake()
        {
            _windowMap = new Dictionary<E_NightWatchMode, NightWatchReportWindow>();
            _backButton.App = _manager;
            foreach (var w in _windows)
            {
                w.Close();
                if (!_windowMap.ContainsKey(w.WindowType))
                    _windowMap.Add(w.WindowType, w);
                else
                    Debug.LogWarning($"Duplicate NightWatch window type: {w.WindowType}");
            }

            _buoysReportController.OnBuoyReportFailed += BuoysOnReportFailed;
        }

        private void OnDestroy()
        {
            _buoysReportController.OnBuoyReportFailed -= BuoysOnReportFailed;
        }

        private void BuoysOnReportFailed(string arg1, bool arg2, bool arg3)
        {
            _signalsController.TryForceRemoveSignal?.Invoke(arg1, arg2, arg3);
        }

        private void OnValidate()
        {
            foreach (var w in _windows)
            {
                w.SetNightWatch(this);
            }
        }


        public override void Open()
        {
            base.Open();
            if (_manager.State == E_ComputerAppState.Opened && _manager.OS.PlayerOnComputer)
            {
                _sonarUIController.StartRadar();
            }
        }

        public override void Close()
        {
            base.Close();
            _sonarUIController.StopRadar();
        }

        private void Start()
        {
            SwitchTo(E_NightWatchMode.Signals);
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
}

