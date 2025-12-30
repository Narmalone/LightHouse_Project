using LightHouse.Game.Computer.Calendar;
using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.Computer.LEO.NightWatch.Boats;
using LightHouse.Game.Computer.LEO.NightWatch.Buoys;
using LightHouse.Game.Computer.LEO.NightWatch.Signals;
using LightHouse.Game.Computer.LEO.NightWatch.Sonar;
using LightHouse.Game.DayNightSystem;
using LightHouse.Game.Nightwatch;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch
{
    public enum E_NightWatchMode { Boat, Buoys, Signals }

    /// <summary>
    /// Nightwatch LEO:
    /// - Fenêtre active dans la plage [StartHour..EndHour).
    /// - UI idempotente pilotée par InRange (robuste aux sauts de temps).
    /// - Automate one-shot: init à l'entrée, recap à la sortie, advance au prochain jour.
    /// </summary>
    public sealed class NightWatchController : LEOWindow
    {
        #region Inspector ─ Wiring

        [SerializeField] private LEOApplication _manager;
        [SerializeField] private TabCanvas _tabCanvas;
        [SerializeField] private CanvasGroup _reportCanvas;            // interactif quand InRange
        [SerializeField] private LEOWindowButton _backButton;
        [SerializeField] private LEOWindowButton _switchToCamera;
        [SerializeField] private NightWatchReportWindow[] _windows;
        [SerializeField] private UI_Sonar _sonarUIController;
        [SerializeField] private UI_BuoysReportController _buoysReportController;
        [SerializeField] private UI_BoatReportController _boatsReportController;
        [SerializeField] private UI_Signals _signalsController;

        [Header("UI States")]
        [SerializeField] private Image _backgroundEndedNightwatch;     // visible hors plage
        [SerializeField] private TextMeshProUGUI _endedNightwatchText;

        [Header("Config")]
        [SerializeField] private SO_NightWatchConfiguration _nightwatchConfig;

        #endregion

        #region State

        private Dictionary<E_NightWatchMode, NightWatchReportWindow> _windowMap;
        private NightWatchReportWindow _activeWindow;

        // Cycle absolu (jours/flags)
        private int _startDay, _endDay;
        private bool _cycleInitialized;
        private bool _cycleCompleted;

        // UI (état précédent pour éviter les updates inutiles)
        private bool _panelWasOpen;

        // Shorthands
        private float StartHour => _nightwatchConfig.StartHour;
        private float EndHour => _nightwatchConfig.EndHour;

        public event Action<MailDatas> SendMailRequested;

        [SerializeField] private CalendarEventDatabase _calendarDatabase;

        #endregion

        #region Unity lifecycle

        protected override void Awake()
        {
            base.Awake();
            _windowMap = new Dictionary<E_NightWatchMode, NightWatchReportWindow>();
            _backButton.App = _manager;

            foreach (var w in _windows)
            {
                w.Close();
                if (!_windowMap.ContainsKey(w.WindowType)) _windowMap.Add(w.WindowType, w);
                else Debug.LogWarning($"Duplicate NightWatch window type: {w.WindowType}");
                w.SetNightWatch(this);
            }

            _buoysReportController.OnBuoyReportFailed += BuoysOnReportFailed;
            TimeHandlerData.OnTimeChanged += OnTimeUpdated;
            CalendarEvent evt = CalendarEventBuilder.New("Nightwatch")
                .Daily()
                .FromTo(_nightwatchConfig.StartHour, _nightwatchConfig.EndHour)
                .Build();
            _calendarDatabase.AddEvent(evt);
        }

        private void Start()
        {
            // UI text
            _endedNightwatchText.text = $"Your job is over for now.\nPlease comeback at {TimeUtility.FormatTime12h(StartHour)}";

            // Ancrage du cycle robuste (traverse minuit incluse)
            ArmCycleForDayFromNow();

            // UI initiale idempotente
            bool inRange = TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime, StartHour, EndHour);
            SetNightwatchInteractive(inRange);
            _panelWasOpen = inRange;

            // Fenêtre par défaut
            SwitchTo(E_NightWatchMode.Signals);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _buoysReportController.OnBuoyReportFailed -= BuoysOnReportFailed;
            TimeHandlerData.OnTimeChanged -= OnTimeUpdated;
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

        #endregion

        #region UI events

        private void BuoysOnReportFailed(string id, bool a, bool b)
        {
            _signalsController.TryForceRemoveSignal?.Invoke(id, a, b);
        }

        public void SwitchTo(E_NightWatchMode target)
        {
            if (_windowMap.TryGetValue(target, out var next))
            {
                if (_activeWindow == next) return;
                _activeWindow?.Close();
                _activeWindow = next;
                _activeWindow.Open();
            }
            else Debug.LogWarning($"No NightWatch window for type {target}");
        }

        #endregion

        #region Cycle ─ Anchoring & Steps

        /// <summary> Détermine l'ancre du cycle en fonction du temps courant (support traverse minuit). </summary>
        private void ArmCycleForDayFromNow()
        {
            int today = TimeHandlerData.CurrentDay;
            float now = TimeHandlerData.CurrentTime;
            bool crossesMidnight = EndHour < StartHour;

            int anchorDay = (crossesMidnight && now < EndHour) ? today - 1 : today;
            ArmCycleForDay(anchorDay);

            InitializeCycleIfNeeded(); // peut init direct si on est déjà après StartHour
        }

        /// <summary> Ancre un nouveau cycle sur 'anchorDay'. </summary>
        private void ArmCycleForDay(int anchorDay)
        {
            _startDay = anchorDay;
            _endDay = _startDay + (EndHour < StartHour ? 1 : 0);
            _cycleInitialized = false;
            _cycleCompleted = false;
        }

        private void InitializeCycleIfNeeded()
        {
            if (_cycleInitialized) return;

            if (HasReachedDateOrPassed(_startDay, StartHour))
            {
                _cycleInitialized = true;

                // Hooks de démarrage (si besoin)
                // _buoysReportController.OnNightwatchStartToday();
                // _boatsReportController.OnNightwatchStartToday();

                // L’UI elle-même est pilotée par InRange à chaque tick (pas ici),
                // mais on peut s’assurer de l’état si nécessaire :
                // SetNightwatchInteractive(true);
            }
        }

        private void CompleteCycleIfNeeded()
        {
            if (_cycleCompleted) return;

            if (HasReachedDateOrPassed(_endDay, EndHour))
            {
                GenerateRecap();
                _cycleCompleted = true;

                // Stop & hooks
                _buoysReportController.OnNightwatchEndedToday();
                _boatsReportController.OnNightwatchEndedToday();

                // L’UI est aussi pilotée par InRange, mais on peut forcer la cohérence :
                // SetNightwatchInteractive(false);
                Debug.Log("[Nightwatch] Recap generated (once).");
            }
        }

        private void AdvanceToNextDayIfNeeded()
        {
            if (!_cycleCompleted) return;

            int nextStartDay = _startDay + 1;
            if (HasReachedDateOrPassed(nextStartDay, StartHour))
            {
                ArmCycleForDay(nextStartDay);
                InitializeCycleIfNeeded();
            }
        }

        #endregion

        #region Tick

        private void OnTimeUpdated(float _)
        {
            // A) UI : toujours pilotée par InRange → couvre les sauts de temps
            bool inRangeNow = TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime, StartHour, EndHour);
            if (inRangeNow != _panelWasOpen)
            {
                SetNightwatchInteractive(inRangeNow);
                _panelWasOpen = inRangeNow;
            }

            // B) Automate one-shot (init / complete / advance)
            AdvanceToNextDayIfNeeded();
            InitializeCycleIfNeeded();
            CompleteCycleIfNeeded();
        }

        #endregion

        #region UI ─ Panel idempotent

        /// <summary>
        /// true  = dans la fenêtre → interactif, fond masqué
        /// false = hors fenêtre → verrouillé, fond visible
        /// </summary>
        private void SetNightwatchInteractive(bool active)
        {
            _backgroundEndedNightwatch.gameObject.SetActive(!active);
            _reportCanvas.interactable = active;
        }

        #endregion

        #region Recap / Mail

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
                arrivalDay: TimeHandlerData.CurrentDay,
                arrivalTime: TimeHandlerData.CurrentTime
            );

            SendMailRequested?.Invoke(mail);
        }

        #endregion

        #region Helpers

        /// <summary> >= et support “jours dépassés” (robuste aux sauts de temps). </summary>
        private static bool HasReachedDateOrPassed(int day, float hour)
        {
            const float EPS = 1e-3f;
            int cd = TimeHandlerData.CurrentDay;
            float ct = TimeHandlerData.CurrentTime;
            if (cd > day) return true;
            if (cd < day) return false;
            return ct + EPS >= hour;
        }

        #endregion
    }
}
