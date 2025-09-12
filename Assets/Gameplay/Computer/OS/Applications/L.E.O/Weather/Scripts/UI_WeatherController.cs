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
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Weather
{
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
            return AirTemperatureResult.Payout + WaterTemperatureResult.Payout + HumidityRateResult.Payout
                + AtmosphericPressureResult.Payout + WindSpeedResult.Payout + WindOrientationResult.Payout;
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

    /// <summary>
    /// Fenêtre de rapport météo côté LEO.
    /// Logique de cycle journalière (start → end → attente prochain start) + calculs de gains.
    /// </summary>
    public sealed class UI_WeatherController : LEOWindow
    {
        #region Serialized Fields ─ UI Wiring

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

        #endregion

        public MoneyWeatherData TodaysWeatherReport { get; private set; }

        public event Action<MailDatas> SendMailRequested;

        // ---- Etat du cycle (même mécanique que Nightwatch) ----
        private int _startDay, _endDay;     // bornes absolues
        private bool _cycleInitialized;     // init faite pour ce cycle ?
        private bool _cycleCompleted;       // recap envoyé pour ce cycle ?

        #region Unity Lifecycle

        private void Awake()
        {
            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            _resetAllButton.onClick.AddListener(OnResetAllClicked);
            TimeHandlerData.OnTimeChanged += OnTimeUpdated;
        }

        private void Start()
        {
            ArmCycleForDayFromNow();   // ancrage robuste si on démarre en pleine nuit
        }

        private void OnDestroy()
        {
            _sendReportButton.onClick.RemoveListener(OnSendReportClicked);
            _resetAllButton.onClick.RemoveListener(OnResetAllClicked);
            TimeHandlerData.OnTimeChanged -= OnTimeUpdated;
        }

        #endregion

        #region UI Events

        /// <summary> Réinitialise tous les contrôleurs d’input. </summary>
        private void OnResetAllClicked()
        {
            _airTemperatureController.SetTemperature(0f);
            _waterTemperatureController.SetTemperature(0f);
            _airPressureController.SetAirPressure(0);

            _windController.SetWindSpeed(0f);
            _windController.CompassController.SelectByDirection(WindOrientationType.North);

            _humidityRateController.SetHumidity01(0f);
        }

        /// <summary>
        /// Envoie le rapport côté UI (graph) puis affiche la popup de résultats.
        /// Ne déclenche pas l’email (ça, c’est l’automate à EndHour).
        /// </summary>
        private void OnSendReportClicked()
        {
            _humidityRateController.SendDatasToGraph(_humidityRateController.CurrentHumidityPercent);
            ShowResultsPopup();
        }

        #endregion

        #region Cycle management (Jour/Heure)

        /// <summary>
        /// Wrapper pour compat ancienne méthode si elle était appelée ailleurs.
        /// </summary>
        public void ComputeWeather() => ArmCycleForDayFromNow();

        /// <summary>
        /// Ancre le cycle sur 'anchorDay'. endDay = +1 si on traverse minuit.
        /// Reset des flags.
        /// </summary>
        private void ArmCycleForDay(int anchorDay)
        {
            _startDay = anchorDay;
            _endDay = _startDay + (_weatherConfig.EndHour < _weatherConfig.StartHour ? 1 : 0);

            _cycleInitialized = false;
            _cycleCompleted = false;
            // Debug.Log($"[Weather] Arm cycle for day={_startDay} (start={_weatherConfig.StartHour}h → endD={_endDay} { _weatherConfig.EndHour}h)");
        }

        /// <summary>
        /// Choisit intelligemment l’ancre en fonction de l’heure actuelle (cold start).
        /// Si on est après minuit mais avant EndHour pour un créneau qui traverse minuit,
        /// on rattache au jour précédent.
        /// </summary>
        private void ArmCycleForDayFromNow()
        {
            int today = TimeHandlerData.CurrentDay;
            float now = TimeHandlerData.CurrentTime;
            bool crossesMidnight = _weatherConfig.EndHour < _weatherConfig.StartHour;

            int anchorDay = today;
            if (crossesMidnight && now < _weatherConfig.EndHour)
                anchorDay = today - 1;

            ArmCycleForDay(anchorDay);

            // Si on est déjà à/au-delà de l'heure de start, considérer l'init faite.
            if (TimeUtility.HasReachedDate(_startDay, _weatherConfig.StartHour))
            {
                _cycleInitialized = true;
                // Préparation éventuelle ici si besoin
            }
        }

        /// <summary> Init quand on atteint StartHour. </summary>
        private void InitializeCycleIfNeeded()
        {
            if (_cycleInitialized) return;

            if (TimeUtility.HasReachedDate(_startDay, _weatherConfig.StartHour))
            {
                _cycleInitialized = true;
                // Reset des données du jour au début de la fenêtre
                TodaysWeatherReport.Reset();
                // Debug.Log("[Weather] Initialized for current cycle.");
            }
        }

        /// <summary> Envoi du récap une seule fois à EndHour. </summary>
        private void CompleteCycleIfNeeded()
        {
            if (_cycleCompleted) return;

            if (TimeUtility.HasReachedDate(_endDay, _weatherConfig.EndHour))
            {
                GenerateRecap();           // email (une seule fois)
                _cycleCompleted = true;
                // Debug.Log("[Weather] Recap generated (once).");
            }
        }

        /// <summary>
        /// Après completion, on n’ouvre le cycle suivant qu’au prochain StartHour.
        /// Pour un créneau 21→04, ça veut dire le **soir même** (jour de 04:00).
        /// Pour un créneau 21→23, ça veut dire **le lendemain** 21:00.
        /// </summary>
        private void AdvanceToNextDayIfNeeded()
        {
            if (!_cycleCompleted) return;

            int nextStartDay = _startDay + 1;

            if (TimeUtility.HasReachedDate(nextStartDay, _weatherConfig.StartHour))
            {
                OnWeatherTimeEnded(); // reset monnaie/données
                ArmCycleForDay(nextStartDay);

                // Si on est déjà à/au-delà du nouveau start, init directe
                if (TimeUtility.HasReachedDate(_startDay, _weatherConfig.StartHour))
                    _cycleInitialized = true;
                // Debug.Log("[Weather] Advanced to next day.");
            }
        }

        private void OnTimeUpdated(float _)
        {
            // Ordre important (même que Nightwatch) :
            // 1) passer au prochain cycle si le précédent est fini et qu'on touche le prochain Start
            AdvanceToNextDayIfNeeded();

            // 2) initialiser quand on atteint le Start du cycle courant
            InitializeCycleIfNeeded();

            // 3) compléter/fermer quand on atteint l'End du cycle courant
            CompleteCycleIfNeeded();
        }

        #endregion

        #region Cycle Hooks

        /// <summary> Appelé lors du passage au cycle suivant (nettoyage/zeroing). </summary>
        public void OnWeatherTimeEnded()
        {
            //TodaysWeatherReport.Reset();
        }

        #endregion

        #region Recap / Mail

        private void GenerateRecap()
        {
            // Toujours recalculer pour être sûr
            //TodaysWeatherReport = CalculateMoney();

            int today = TimeHandlerData.CurrentDay;

            // “Blended” par notre fonction GetForecastBasedOnPlayerInput
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

            SendMailRequested?.Invoke(weatherMail);
        }


        #endregion

        #region Popup & Report Rendering

        /// <summary>
        /// Instancie la popup puis génère le contenu une fois "chargée".
        /// </summary>
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

        /// <summary>
        /// Construit les lignes du récap : eau / air / pression / vent / humidité + total.
        /// </summary>
        private void GenerateReportElements(UI_ReportDatasPopup popup, RectTransform parent, MoneyWeatherData datas)
        {
            if (WeatherHandlerData.CurrentWeather == null)
            {
                CreateReportElement(parent, "Weather not available", "0", Color.gray);
                return;
            }

            // Lignes détaillées
            var lines = new (string label, float amount)[]
            {
                ("Water temperature",  datas.WaterTemperatureResult.Payout),
                ("Air temperature",    datas.AirTemperatureResult.Payout),
                ("Air pressure",       datas.AtmosphericPressureResult.Payout),
                ("Wind speed",         datas.WindSpeedResult.Payout),
                ("Wind direction",     datas.WindOrientationResult.Payout),
                ("Humidity",           datas.HumidityRateResult.Payout),
            };

            foreach (var (label, amount) in lines)
            {
                CreateReportElement(
                    parent,
                    description: label,
                    amount: $"{WeatherMoneyCalculator.FormatMoney(amount)}$",
                    color: WeatherMoneyCalculator.ColorForAmount(amount)
                );
            }

            var totalEarned = datas.GetTotalPayout();
            CreateReportElement(
                parent,
                description: "Total",
                amount: $"{WeatherMoneyCalculator.FormatMoney(totalEarned)}$",
                color: WeatherMoneyCalculator.ColorForAmount(totalEarned)
            );
        }

        /// <summary> Instancie un élément de ligne de rapport. </summary>
        private void CreateReportElement(RectTransform parent, string description, string amount, Color color)
        {
            var element = Instantiate(_reportElementPrefab, parent);
            element.SetDescription(description);
            element.SetMoneyResult(amount, color);
        }

        #endregion

        #region Money Computation

        /// <summary>
        /// Calcule les gains individuels et le total pour les catégories météo.
        /// </summary>
        private MoneyWeatherData CalculateMoney()
        {
            var w = WeatherHandlerData.CurrentWeather;

            WeatherPayoutResult waterTempResult = WeatherMoneyCalculator.CalculateWaterTemperature(w.WaterTemperature, _waterTemperatureController.CurrentTemperature, _weatherMoneyResult);
            WeatherPayoutResult airTempResult = WeatherMoneyCalculator.CalculateAirTemperature(w.AirTemperature, _airTemperatureController.CurrentTemperature, _weatherMoneyResult);
            WeatherPayoutResult atmosphericPressureResult = WeatherMoneyCalculator.CalculateAirPressure(w.AtmosphericPressure, _airPressureController.CurrentRange, _weatherMoneyResult);
            WeatherPayoutResult windSpeedResult = WeatherMoneyCalculator.CalculateWindSpeed(w.WindSpeed, _windController.CurrentWindSpeed, _weatherMoneyResult);
            WeatherPayoutResult windOrientationResult = WeatherMoneyCalculator.CalculateWindDirection(w.WindOrientationType, _windController.CompassController.CurrentSelectedOrientation, _weatherMoneyResult);
            WeatherPayoutResult humidityResult = WeatherMoneyCalculator.CalculateHumidity(w.Humidity, _humidityRateController.CurrentHumidityPercent, _weatherMoneyResult);

            float total = waterTempResult.Payout + airTempResult.Payout + atmosphericPressureResult.Payout + windSpeedResult.Payout + windOrientationResult.Payout + humidityResult.Payout;
            Debug.Log($"[ACC] Air={airTempResult.Accuracy} Water={waterTempResult.Accuracy} Hum={humidityResult.Accuracy} WindSpd={windSpeedResult.Accuracy} WindDir={windOrientationResult.Accuracy} Press={atmosphericPressureResult.Accuracy}");

            return new MoneyWeatherData
            {
                AirTemperatureResult = airTempResult,
                WaterTemperatureResult = waterTempResult,
                AtmosphericPressureResult = atmosphericPressureResult,
                WindSpeedResult = windSpeedResult,
                WindOrientationResult = windOrientationResult,
                HumidityRateResult = humidityResult,
            };
        }
        
        [SerializeField] private float _emailTempHalfBandC = 2.0f;
        private MailGenerator.ForecastLine LineFromWeather(
        string periodLabel,
        LightHouse.Weather.WeatherData w,
        float confidencePct)
        {
            if (w == null) return null;

            // Plage T° configurable autour de la T° mesurée du créneau
            float half = Mathf.Max(0f, _emailTempHalfBandC);
            int low = Mathf.RoundToInt(w.AirTemperature - half);
            int high = Mathf.RoundToInt(w.AirTemperature + half);

            int wind = Mathf.RoundToInt(w.WindSpeed);

            string sea = "";
            if (_beaufortScale.FindBeaufortDatasByWindSpeed(wind, out BeaufortScale beaufortDatas))
            {
                sea = beaufortDatas.Description;
            }
            string dir = WeatherUtils.GetCardinalLetter(w.WindOrientationType);

            return new MailGenerator.ForecastLine(
                periodLabel, low, high, wind, sea,
                note: "", windDir: dir, confidencePct: confidencePct
            );
        }

        #endregion
    }
}
