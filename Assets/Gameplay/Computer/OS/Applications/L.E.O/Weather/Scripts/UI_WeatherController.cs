using LightHouse.Game.Computer.Calendar;
using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.Computer.LEO.Weather.Humidity;
using LightHouse.Game.Computer.LEO.Weather.Pressure;
using LightHouse.Game.Computer.LEO.Weather.Temperature;
using LightHouse.Game.Computer.LEO.Weather.Wind;
using LightHouse.Game.DayNightSystem;
using LightHouse.Money;
using LightHouse.Weather;
using LightHouse.Weather.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Weather
{
    #region Data

    /// <summary>
    /// Regroupe les résultats de payout par catégorie météo pour une journée.
    /// </summary>
    public struct MoneyWeatherData
    {
        public WeatherPayoutResult AirTemperatureResult;
        public WeatherPayoutResult WaterTemperatureResult;
        public WeatherPayoutResult HumidityRateResult;
        public WeatherPayoutResult AtmosphericPressureResult;
        public WeatherPayoutResult WindSpeedResult;
        public WeatherPayoutResult WindOrientationResult;

        public float GetTotalPayout()
        {
            return AirTemperatureResult.Payout
                 + WaterTemperatureResult.Payout
                 + HumidityRateResult.Payout
                 + AtmosphericPressureResult.Payout
                 + WindSpeedResult.Payout
                 + WindOrientationResult.Payout;
        }

        public void Reset()
        {
            AirTemperatureResult = new WeatherPayoutResult();
            WaterTemperatureResult = new WeatherPayoutResult();
            HumidityRateResult = new WeatherPayoutResult();
            AtmosphericPressureResult = new WeatherPayoutResult();
            WindSpeedResult = new WeatherPayoutResult();
            WindOrientationResult = new WeatherPayoutResult();
        }
    }

    #endregion

    /// <summary>
    /// Fenêtre de rapport météo côté LEO.
    /// - Gère un cycle journalier [StartHour..EndHour) : init à l'entrée de fenêtre, recap à la sortie.
    /// - UI panneau pilotée à chaque tick par InRange (idempotent → safe en cas de saut de temps).
    /// - Calcul des gains + génération d'email récap.
    /// </summary>
    public sealed class UI_WeatherController : LEOWindow
    {
        #region Inspector ─ Wiring

        [Header("Actions")]
        [SerializeField] private Button _sendReportButton;
        [SerializeField] private Button _resetAllButton;

        [Header("Inputs")]
        [SerializeField] private UI_WeatherTemperatureController _airTemperatureController;
        [SerializeField] private UI_AirPressureController _airPressureController;
        [SerializeField] private UI_WeatherTemperatureController _waterTemperatureController;
        [SerializeField] private UI_WindWindowController _windController;
        [SerializeField] private UI_HumidityRateController _humidityRateController;

        [Header("Configs")]
        [SerializeField] private SO_WeatherMoneyResults _weatherMoneyResult;
        [SerializeField] private SO_WeatherConfigurations _weatherConfig;
        [SerializeField] private WeatherTimeline _weatherTimeline;
        [SerializeField] private WeatherConfigDatabase _weatherConfigDatabase;
        [SerializeField] private SO_BeaufortScale _beaufortScale;

        [Header("Popup / UI")]
        [SerializeField] private UI_ReportDatasPopup _reportDatasPopupPrefab;
        [SerializeField] private UI_ReportElement _reportElementPrefab;
        [SerializeField] private Image _backgroundEndedWeather;
        [SerializeField] private TextMeshProUGUI _endedWeatherText;

        [SerializeField] private CalendarEventDatabase _calendarDatabase;

        #endregion

        #region State

        public MoneyWeatherData TodaysWeatherReport { get; private set; }
        public event Action<MailDatas> SendMailRequested;

        // Cycle (bornes absolues)
        private int _startDay, _endDay;
        private bool _cycleInitialized;
        private bool _cycleCompleted;

        // UI
        private bool _panelWasOpen; // vrai si la fenêtre [Start..End) est active à la frame précédente

        // Shorthands (pour lisibilité)
        private float StartHour => _weatherConfig.StartHour;
        private float EndHour => _weatherConfig.EndHour;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            _resetAllButton.onClick.AddListener(OnResetAllClicked);
            TimeHandlerData.OnTimeChanged += OnTimeUpdated;
        }

        private void Start()
        {
            ArmCycleForDayFromNow();

            _endedWeatherText.text = "The weather report is closed for now.\n" +
                $"Please comeback at {TimeUtility.FormatTime12h(StartHour)}";

            // UI initiale, idempotente
            bool inRange = TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime, StartHour, EndHour);
            SetPanelInteractive(inRange);
            _panelWasOpen = inRange;

            CalendarEvent evt = CalendarEventBuilder.New("Weather Report")
                .Daily()
                .FromTo(_weatherConfig.StartHour, _weatherConfig.EndHour)
                .Build();
            _calendarDatabase.AddEvent(evt);
            // Optionnel : forcer une passe logique complète si nécessaire
            // OnTimeUpdated(0f);
        }

        private void OnDestroy()
        {
            _sendReportButton.onClick.RemoveListener(OnSendReportClicked);
            _resetAllButton.onClick.RemoveListener(OnResetAllClicked);
            TimeHandlerData.OnTimeChanged -= OnTimeUpdated;
        }

        #endregion

        #region UI ─ Events

        private void OnResetAllClicked()
        {
            _airTemperatureController.SetTemperature(0f);
            _waterTemperatureController.SetTemperature(0f);
            _airPressureController.SetAirPressure(0);

            _windController.SetWindSpeed(0f);
            _windController.CompassController.SelectByDirection(WindOrientationType.North);

            _humidityRateController.SetHumidity01(0f);
        }

        private void OnSendReportClicked()
        {
            _humidityRateController.SendDatasToGraph(_humidityRateController.CurrentHumidityPercent);
            ShowResultsPopup();
        }

        #endregion

        #region Cycle ─ Anchoring & Steps

        /// <summary> Appel legacy si besoin ailleurs. </summary>
        public void ComputeWeather() => ArmCycleForDayFromNow();

        /// <summary>
        /// Ancre le cycle sur 'anchorDay'. Si la fenêtre traverse minuit, endDay = startDay + 1.
        /// </summary>
        private void ArmCycleForDay(int anchorDay)
        {
            _startDay = anchorDay;
            _endDay = _startDay + (EndHour < StartHour ? 1 : 0);
            _cycleInitialized = false;
            _cycleCompleted = false;
        }

        /// <summary>
        /// Fige l'ancre en fonction du temps courant (cold start robuste, y compris traverse minuit).
        /// </summary>
        private void ArmCycleForDayFromNow()
        {
            int today = TimeHandlerData.CurrentDay;
            float now = TimeHandlerData.CurrentTime;
            bool crossesMidnight = EndHour < StartHour;

            int anchorDay = (crossesMidnight && now < EndHour) ? today - 1 : today;
            ArmCycleForDay(anchorDay);

            InitializeCycleIfNeeded(); // peut s'auto-initialiser si on est déjà après StartHour
        }

        /// <summary> Init (une fois) quand on atteint/dépasse StartHour. </summary>
        private void InitializeCycleIfNeeded()
        {
            if (_cycleInitialized) return;

            if (HasReachedDateOrPassed(_startDay, StartHour))
            {
                _cycleInitialized = true;
                TodaysWeatherReport.Reset();
                SetPanelInteractive(true); // entrée dans la fenêtre → interactif
            }
        }

        /// <summary> Recap (une fois) quand on atteint/dépasse EndHour. </summary>
        private void CompleteCycleIfNeeded()
        {
            if (_cycleCompleted) return;

            if (HasReachedDateOrPassed(_endDay, EndHour))
            {
                GenerateRecap();
                _cycleCompleted = true;
                SetPanelInteractive(false); // sortie de la fenêtre → verrouillé
            }
        }

        /// <summary>
        /// Passe au prochain cycle une fois le précédent complété et le prochain Start atteint.
        /// </summary>
        private void AdvanceToNextDayIfNeeded()
        {
            if (!_cycleCompleted) return;

            int nextStartDay = _startDay + 1;

            if (HasReachedDateOrPassed(nextStartDay, StartHour))
            {
                OnWeatherTimeEnded(); // hook nettoyage si besoin
                ArmCycleForDay(nextStartDay);
                InitializeCycleIfNeeded();
            }
        }

        #endregion

        #region UI ─ Panel

        /// <summary>
        /// Applique l’état du panneau selon la fenêtre (idempotent, sans side effects).
        /// true  = dans la fenêtre → interactif
        /// false = hors fenêtre → verrouillé
        /// </summary>
        private void SetPanelInteractive(bool isInRange)
        {
            _backgroundEndedWeather.gameObject.SetActive(!isInRange);
            _sendReportButton.interactable = isInRange;
            _resetAllButton.interactable = isInRange;
        }

        #endregion

        #region Tick

        private void OnTimeUpdated(float _)
        {
            // A. UI : toujours pilotée par InRange (couvre les sauts de temps)
            bool inRangeNow = TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime, StartHour, EndHour);
            if (inRangeNow != _panelWasOpen)
            {
                SetPanelInteractive(inRangeNow);
                _panelWasOpen = inRangeNow;
            }

            // B. Cycle (événements one-shot)
            AdvanceToNextDayIfNeeded();
            InitializeCycleIfNeeded();
            CompleteCycleIfNeeded();
        }

        #endregion

        #region Hooks

        /// <summary> Appelé lors du passage au cycle suivant (nettoyage/zeroing). </summary>
        public void OnWeatherTimeEnded()
        {
            // Ex: TodaysWeatherReport.Reset();
        }

        #endregion

        #region Recap / Mail

        private void GenerateRecap()
        {
            int today = TimeHandlerData.CurrentDay;

            var todayW = _weatherTimeline.Forecast.GetForecastBasedOnPlayerInput(today, TodaysWeatherReport, TimeOfDaySegment.Morning);
            todayW.WeatherType = WeatherUtils.DetermineWeatherType(todayW.Humidity, todayW.AtmosphericPressure, todayW.AirTemperature, todayW.WindSpeed, todayW.WaterTemperature, _weatherConfigDatabase);

            var tmrW = _weatherTimeline.Forecast.GetForecastBasedOnPlayerInput(today + 1, TodaysWeatherReport, TimeOfDaySegment.Morning);
            tmrW.WeatherType = WeatherUtils.DetermineWeatherType(tmrW.Humidity, tmrW.AtmosphericPressure, tmrW.AirTemperature, tmrW.WindSpeed, tmrW.WaterTemperature, _weatherConfigDatabase);

            var d2W = _weatherTimeline.Forecast.GetForecastBasedOnPlayerInput(today + 2, TodaysWeatherReport, TimeOfDaySegment.Morning);
            d2W.WeatherType = WeatherUtils.DetermineWeatherType(d2W.Humidity, d2W.AtmosphericPressure, d2W.AirTemperature, d2W.WindSpeed, d2W.WaterTemperature, _weatherConfigDatabase);

            float avgAcc = _weatherTimeline.Forecast.AverageAccuracy(TodaysWeatherReport);

            var lines = new List<MailGenerator.ForecastLine>
            {
                LineFromWeather("Morning", todayW, avgAcc),
                LineFromWeather("Morning", tmrW,   avgAcc),
                LineFromWeather("Morning", d2W,    avgAcc),
            };

            var weatherMail = MailGenerator.GenerateMailFromWeatherTemplate(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "[Keepers Name]",
                airTempAcc: TodaysWeatherReport.AirTemperatureResult.Accuracy,
                waterTempAcc: TodaysWeatherReport.WaterTemperatureResult.Accuracy,
                humidityAcc: TodaysWeatherReport.HumidityRateResult.Accuracy,
                windSpeedAcc: TodaysWeatherReport.WindSpeedResult.Accuracy,
                windDirectionAcc: TodaysWeatherReport.WindOrientationResult.Accuracy,
                pressureAcc: TodaysWeatherReport.AtmosphericPressureResult.Accuracy,
                totalEarnings: TodaysWeatherReport.GetTotalPayout(),
                forecast: lines,
                accuracyThreshold: 40f
            );

            Debug.Log("[Weather] Recap generated");
            SendMailRequested?.Invoke(weatherMail);
        }

        private MailGenerator.ForecastLine LineFromWeather(
            string periodLabel,
            LightHouse.Weather.WeatherData w,
            float confidencePct)
        {
            if (w == null) return null;

            int wind = Mathf.RoundToInt(w.WindSpeed);

            string sea = "";
            if (_beaufortScale.FindBeaufortDatasByWindSpeed(wind, out BeaufortScale beaufortDatas))
                sea = beaufortDatas.Description;

            // Bande de T° configurable autour de la T° mesurée
            int low = Mathf.RoundToInt(w.AirTemperature - Mathf.Max(0f, _emailTempHalfBandC));
            int high = Mathf.RoundToInt(w.AirTemperature + Mathf.Max(0f, _emailTempHalfBandC));

            string dir = WeatherUtils.GetCardinalLetter(w.WindOrientationType);

            return new MailGenerator.ForecastLine(
                periodLabel, low, high, wind, sea,
                note: "", windDir: dir, confidencePct: confidencePct
            );
        }

        [SerializeField] private float _emailTempHalfBandC = 2.0f;

        #endregion

        #region Popup & Report Rendering

        private void ShowResultsPopup()
        {
            var popup = Instantiate(_reportDatasPopupPrefab, (RectTransform)transform);
            (popup.transform as RectTransform).anchoredPosition = Vector3.zero;

            popup.OnLoadingCompleted += status =>
            {
                if (status == DataStatus.Success)
                {
                    TodaysWeatherReport = CalculateMoney();
                    PlayerCurrency.Add(TodaysWeatherReport.GetTotalPayout());
                    GenerateReportElements(popup, popup.BodyParentContent, TodaysWeatherReport);
                    popup.RefreshLayouts();
                }
            };

            popup.StartLoading(() => DataStatus.Success);
        }

        private void GenerateReportElements(UI_ReportDatasPopup popup, RectTransform parent, MoneyWeatherData datas)
        {
            if (WeatherHandlerData.CurrentWeather == null)
            {
                CreateReportElement(parent, "Weather not available", "0", Color.gray);
                return;
            }

            var rows = new (string label, float amount)[]
            {
                ("Water temperature",  datas.WaterTemperatureResult.Payout),
                ("Air temperature",    datas.AirTemperatureResult.Payout),
                ("Air pressure",       datas.AtmosphericPressureResult.Payout),
                ("Wind speed",         datas.WindSpeedResult.Payout),
                ("Wind direction",     datas.WindOrientationResult.Payout),
                ("Humidity",           datas.HumidityRateResult.Payout),
            };

            foreach (var (label, amount) in rows)
            {
                CreateReportElement(
                    parent,
                    description: label,
                    amount: $"{WeatherMoneyCalculator.FormatMoney(amount)}$",
                    color: WeatherMoneyCalculator.ColorForAmount(amount)
                );
            }

            var total = datas.GetTotalPayout();
            CreateReportElement(parent, "Total", $"{WeatherMoneyCalculator.FormatMoney(total)}$", WeatherMoneyCalculator.ColorForAmount(total));
        }

        private void CreateReportElement(RectTransform parent, string description, string amount, Color color)
        {
            var element = Instantiate(_reportElementPrefab, parent);
            element.SetDescription(description);
            element.SetMoneyResult(amount, color);
        }

        #endregion

        #region Money

        private MoneyWeatherData CalculateMoney()
        {
            var w = WeatherHandlerData.CurrentWeather;

            var waterTempResult = WeatherMoneyCalculator.CalculateWaterTemperature(w.WaterTemperature, _waterTemperatureController.CurrentTemperature, _weatherMoneyResult);
            var airTempResult = WeatherMoneyCalculator.CalculateAirTemperature(w.AirTemperature, _airTemperatureController.CurrentTemperature, _weatherMoneyResult);
            var atmosphericPressureRes = WeatherMoneyCalculator.CalculateAirPressure(w.AtmosphericPressure, _airPressureController.CurrentRange, _weatherMoneyResult);
            var windSpeedResult = WeatherMoneyCalculator.CalculateWindSpeed(w.WindSpeed, _windController.CurrentWindSpeed, _weatherMoneyResult);
            var windOrientationResult = WeatherMoneyCalculator.CalculateWindDirection(w.WindOrientationType, _windController.CompassController.CurrentSelectedOrientation, _weatherMoneyResult);
            var humidityResult = WeatherMoneyCalculator.CalculateHumidity(w.Humidity, _humidityRateController.CurrentHumidityPercent, _weatherMoneyResult);

            Debug.Log($"[ACC] Air={airTempResult.Accuracy} Water={waterTempResult.Accuracy} Hum={humidityResult.Accuracy} WindSpd={windSpeedResult.Accuracy} WindDir={windOrientationResult.Accuracy} Press={atmosphericPressureRes.Accuracy}");

            return new MoneyWeatherData
            {
                AirTemperatureResult = airTempResult,
                WaterTemperatureResult = waterTempResult,
                AtmosphericPressureResult = atmosphericPressureRes,
                WindSpeedResult = windSpeedResult,
                WindOrientationResult = windOrientationResult,
                HumidityRateResult = humidityResult,
            };
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Version tolérante de HasReachedDate (>= et support des jours dépassés)
        /// </summary>
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
