using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.Computer.LEO.NightWatch.Boats;
using LightHouse.Game.Computer.LEO.NightWatch.Buoys;
using LightHouse.Game.Computer.LEO.NightWatch.Signals;
using LightHouse.Game.Computer.LEO.NightWatch.Sonar;
using LightHouse.Game.DayNightSystem;
using LightHouse.Game.Nightwatch;
using System;
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
        [SerializeField] private UI_BoatReportController _boatsReportController;
        [SerializeField] private UI_Signals _signalsController;

        [SerializeField] private SO_NightWatchConfiguration _nightwatchConfig;

        private Dictionary<E_NightWatchMode, NightWatchReportWindow> _windowMap;
        private NightWatchReportWindow _activeWindow;

        private bool _isReportDoneToday = false;
        private MailDatas CurrentNightWatchRecap;
        public event Action<MailDatas> PleaseSendReport;

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
            TimeHandlerData.OnTimeChanged += OnTimeUpdated;
        }

        private void OnDestroy()
        {
            _buoysReportController.OnBuoyReportFailed -= BuoysOnReportFailed;
            TimeHandlerData.OnTimeChanged -= OnTimeUpdated;
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

        private int _startDay, _endDay;

        private void ComputerNightwatch()
        {
            _startDay = TimeHandlerData.CurrentDay;
            if (_nightwatchConfig.EndHour < _nightwatchConfig.StartHour)
            {
                _endDay = _startDay + 1;
            }
            else _endDay = _startDay;

        }

        private void OnTimeUpdated(float obj)
        {
            if (!_isReportDoneToday && TimeUtility.HasReachedDate(_endDay, _nightwatchConfig.EndHour))
            {
                GenerateRecap();
                _isReportDoneToday = true;
                _buoysReportController.OnNightwatchEndedToday();
                _boatsReportController.OnNightwatchEndedToday();
            }
            else if(_isReportDoneToday && TimeUtility.HasReachedDate(_endDay, _nightwatchConfig.StartHour))
            {
                _isReportDoneToday = false;
                ComputerNightwatch();
            }
        }

        public void GenerateRecap()
        {
            BuoyReportResult buoyTodaysResult = _buoysReportController.GetTodaysResult();
            MoneyAllBoatsBreakdown boatTodaysResult = _boatsReportController.GetTodaysResult();

            var mail = MailGenerator.GenerateMailFromNightwatchTemplate(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "{Keepers Name}",
                boatsCorrect: boatTodaysResult.AllBoats.Count,
                boatsErrors: boatTodaysResult.GetTotalNumberOfTry(),
                buoysNominal: buoyTodaysResult.CorrectValidCount,
                buoysDefective: buoyTodaysResult.CorrectInvalidCount,
                buoysErrors: buoyTodaysResult.ErrorCount,
                totalEarnings: boatTodaysResult.GetGrandTotal() + buoyTodaysResult.TotalEarnedDuringTheNight,
                captainsNote: "",
                TimeHandlerData.CurrentDay,
                TimeHandlerData.CurrentTime
                );

            //var mail = MailGenerator.GenerateMailFromNightwatchTemplate();

            PleaseSendReport?.Invoke(mail);
        }

        public override void Open()
        {
            base.Open();
            _sonarUIController.StartRadar();
        }

        public override void Close()
        {
            base.Close();
            _sonarUIController.StopRadar();
        }

        private void Start()
        {
            ComputerNightwatch();
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

