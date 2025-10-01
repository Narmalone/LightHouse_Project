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
        [SerializeField] private CanvasGroup _reportCanvas;
        [SerializeField] private LEOWindowButton _backButton;
        [SerializeField] private LEOWindowButton _switchToCamera;
        [SerializeField] private NightWatchReportWindow[] _windows;
        [SerializeField] private UI_Sonar _sonarUIController;
        [SerializeField] private UI_BuoysReportController _buoysReportController;
        [SerializeField] private UI_BoatReportController _boatsReportController;
        [SerializeField] private UI_Signals _signalsController;
        [SerializeField] private Image _backgroundEndedNightwatch;
        [SerializeField] private TextMeshProUGUI _endedNightwatchText;

        [SerializeField] private SO_NightWatchConfiguration _nightwatchConfig;

        private Dictionary<E_NightWatchMode, NightWatchReportWindow> _windowMap;
        private NightWatchReportWindow _activeWindow;

        // ---- Etat de cycle journalier ----
        private int _startDay, _endDay;                 // bornes absolues (jour)
        private bool _cycleInitialized = false;         // initialisation effectuée pour ce cycle ?
        private bool _cycleCompleted = false;           // recap déjà envoyé pour ce cycle ?

        public event Action<MailDatas> SendMailRequested;

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

        private void OnValidate()
        {
            foreach (var w in _windows)
                w.SetNightWatch(this);
        }

        private void Start()
        {
            ArmCycleForDay(TimeHandlerData.CurrentDay);
            SwitchTo(E_NightWatchMode.Signals);
            
            var time = TimeUtility.FormatTime12h(_nightwatchConfig.StartHour);
            _endedNightwatchText.text = $"Your job is over for now. \n Please comeback at {time} ";
            bool mustEnableNightwatchOnStart = TimeHandlerData.CurrentTime > _nightwatchConfig.StartHour;
            _backgroundEndedNightwatch.gameObject.SetActive(!mustEnableNightwatchOnStart);
            _reportCanvas.interactable = mustEnableNightwatchOnStart;
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

        private void BuoysOnReportFailed(string id, bool arg2, bool arg3)
        {
            _signalsController.TryForceRemoveSignal?.Invoke(id, arg2, arg3);
        }

        private void EnableReportPannel()
        {
            _backgroundEndedNightwatch.gameObject.SetActive(true);
        }

        private void DisableReportPannel()
        {
            _backgroundEndedNightwatch.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------------------
        // Cycle management
        // ------------------------------------------------------------------------------

        /// <summary>
        /// Ancre un nouveau cycle sur 'anchorDay'.
        /// EndDay = anchorDay (+1 si EndHour &lt; StartHour, i.e. cycle qui traverse minuit).
        /// Reset des flags d'état.
        /// </summary>
        private void ArmCycleForDay(int anchorDay)
        {
            _startDay = anchorDay;
            _endDay = _startDay + (_nightwatchConfig.EndHour < _nightwatchConfig.StartHour ? 1 : 0);

            _cycleInitialized = false;
            _cycleCompleted = false;

            // Debug
            // Debug.Log($"[Nightwatch] Arm cycle for day={_startDay} (start={_nightwatchConfig.StartHour}h, endDay={_endDay}, end={_nightwatchConfig.EndHour}h)");
        }

        /// <summary> S'exécute quand on entre dans la fenêtre [start..end). </summary>
        private void InitializeCycleIfNeeded()
        {
            if (_cycleInitialized) return;

            if (TimeUtility.HasReachedDate(_startDay, _nightwatchConfig.StartHour))
            {
                _cycleInitialized = true;
                _reportCanvas.interactable = true;
                DisableReportPannel();
                Debug.Log("disable report pannel");
                // Place pour reinit/prepare tes systèmes si besoin (reset de compteurs, etc.)
                // _buoysReportController.OnNightwatchStartToday(); // si tu as ce hook
                // _boatsReportController.OnNightwatchStartToday(); // si tu as ce hook
                // Debug.Log("[Nightwatch] Initialized for current cycle.");
            }
        }

        /// <summary> S'exécute UNE SEULE FOIS à l'instant de fin. </summary>
        private void CompleteCycleIfNeeded()
        {
            if (_cycleCompleted) return;

            if (TimeUtility.HasReachedDate(_endDay, _nightwatchConfig.EndHour))
            {
                // IMPORTANT : si StartHour == EndHour, on peut arriver ici au même tick que l'init.
                // L'ordre des appels dans OnTimeUpdated garantit qu'on init d'abord, puis on close.
                GenerateRecap();
                _cycleCompleted = true;

                //Faire un reset de la nightwatch, stopper absolument toutes les anomalies,
                //lancer l'echec du joueur ou la réussite
                _reportCanvas.interactable = false;
                EnableReportPannel();

                _buoysReportController.OnNightwatchEndedToday();
                _boatsReportController.OnNightwatchEndedToday();
                Debug.Log("[Nightwatch] Recap generated (once).");
            }
        }

        /// <summary>
        /// Passe au cycle du lendemain quand on atteint le prochain start.
        /// Ré-initialisation retardée au lendemain uniquement (évite les boucles).
        /// </summary>
        private void AdvanceToNextDayIfNeeded()
        {
            if (!_cycleCompleted) return; // On n'avance au lendemain que si le cycle courant est terminé

            int nextStartDay = _startDay + 1;

            if (TimeUtility.HasReachedDate(nextStartDay, _nightwatchConfig.StartHour))
            {
                // Armer le nouveau cycle pour le lendemain
                ArmCycleForDay(nextStartDay);

                // On peut initialiser immédiatement si on est déjà à/au-delà de l'heure de start
                InitializeCycleIfNeeded();
                // Debug.Log("[Nightwatch] Advanced to next day and (re)initialized.");
            }
        }

        private void OnTimeUpdated(float _)
        {
            // 1) Si le cycle d'hier est terminé et qu'on a atteint le START du lendemain -> armer + init
            AdvanceToNextDayIfNeeded();

            // 2) Initialiser quand on atteint la borne de départ du cycle courant
            InitializeCycleIfNeeded();

            // 3) Clore quand on atteint la borne de fin du cycle courant (une seule fois)
            CompleteCycleIfNeeded();
        }

        // ------------------------------------------------------------------------------
        // Récap & UI
        // ------------------------------------------------------------------------------

        public void GenerateRecap()
        {
            BuoyReportResult buoyTodaysResult = _buoysReportController.GetTodaysResult();
            MoneyAllBoatsBreakdown boatTodaysResult = _boatsReportController.GetTodaysResult();

            MailDatas mail = MailGenerator.GenerateMailFromNightwatchTemplate(
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
