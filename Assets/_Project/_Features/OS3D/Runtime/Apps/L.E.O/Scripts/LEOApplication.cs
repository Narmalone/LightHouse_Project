using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.Computer.LEO.NightWatch;
using LightHouse.Game.Computer.LEO.Weather;
using LightHouse.Game.Computer.LEO.Supplies;
using LightHouse.Game.Computer.OS;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LightHouse.Game.Computer.LEO
{
    public enum ELEOWindow
    {
        Menu,
        NightWatch,
        CameraView,
        Meteo,
        Mails,
        Supplies,
        Maintenance
    }

    public class LEOApplication : ComputerApp
    {
        [SerializeField] private TabCanvas _tabCanvas;
        [SerializeField] private LEOWindow[] _windows;
        [SerializeField] private LEOWindowButton[] _windowsButton;
        [SerializeField] private NightWatchController _nightWatchController;
        [SerializeField] private UI_WeatherController _weatherController;
        [SerializeField] private SupplyManager _supplyManager;
        [SerializeField] private UI_Mails _mailsController;

        private Dictionary<ELEOWindow, LEOWindow> _windowMap;
        public LEOWindow CurrentActiveWindow { get; private set; }
        public NightWatchController NightWatch => _nightWatchController;
        public UI_WeatherController UI_Weather => _weatherController;

        protected override void Awake()
        {
            base.Awake();
            _windows = GetComponentsInChildren<LEOWindow>();
            _windowsButton = GetComponentsInChildren<LEOWindowButton>();

            _windowMap = new Dictionary<ELEOWindow, LEOWindow>();
            _nightWatchController.SendMailRequested += NightWatchController_SendMailRequested;
            _weatherController.SendMailRequested += WeatherController_SendMailRequested;
            _supplyManager.SendMailRequest += SupplyManager_SendMailRequested;

            foreach(var btn in _windows)
            {
                btn.OnWindowClosed += OnCloseCliqued;
            }

            foreach (var windowButton in _windowsButton)
            {
                windowButton.App = this;
            }
        }

        private void OnCloseCliqued()
        {
            this.OnClose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _nightWatchController.SendMailRequested -= NightWatchController_SendMailRequested;
            _weatherController.SendMailRequested -= WeatherController_SendMailRequested;
            _supplyManager.SendMailRequest -= SupplyManager_SendMailRequested;

            foreach (var btn in _windows)
            {
                btn.OnWindowClosed -= OnCloseCliqued;
            }
        }

        private void SupplyManager_SendMailRequested(MailDatas obj)
        {
            _mailsController.GenerateMail(obj);
        }

        private void NightWatchController_SendMailRequested(MailDatas datas)
        {
            _mailsController.GenerateMail(datas);
        }
        private void WeatherController_SendMailRequested(MailDatas datas)
        {
            _mailsController.GenerateMail(datas);
        }

        public override void Initialize(OS.OS os)
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

        public override void OnOpen(bool playSound = true)
        {
            _tabCanvas.EnableCanvasGroup();

            if(ServiceLocator.Audio != null && _onOpenSound && playSound)
                ServiceLocator.Audio.PlayAt(_onOpenSound, this.transform.position);

            ShowWindow(ELEOWindow.Menu);
        }

        public override void OnClose(bool playSound = true)
        {
            if (ServiceLocator.Audio != null && _onCloseSound && playSound)
                ServiceLocator.Audio.PlayAt(_onCloseSound, this.transform.position);
            if (isActiveAndEnabled) State = E_ComputerAppState.Closed;
            CurrentActiveWindow = null;
            _tabCanvas.DisableCanvasGroup();
        }
        public override void OnMinimize() { }
    }
}

