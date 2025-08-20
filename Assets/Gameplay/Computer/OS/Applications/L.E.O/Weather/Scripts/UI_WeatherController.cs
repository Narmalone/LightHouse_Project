using LightHouse.Game.Computer.LEO.Weather.Temperature;
using LightHouse.Game.Computer.LEO.Weather.Pressure;
using LightHouse.Game.Computer.LEO.Weather.Wind;
using LightHouse.Game.Computer.LEO.Weather.Humidity;
using LightHouse.Money;
using LightHouse.Weather;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Weather
{
    /// <summary>
    /// Fenêtre de rapport météo côté LEO.
    /// Récupère les inputs de l’utilisateur, calcule les gains par catégorie et affiche un récap.
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

        [Header("Scoring Config")]
        [SerializeField] private SO_WeatherMoneyResults _weatherMoneyResult;

        [Header("Popup / UI")]
        [SerializeField] private UI_ReportDatasPopup _reportDatasPopupPrefab;
        [SerializeField] private UI_ReportElement _reportElementPrefab;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Abonnements boutons
            _sendReportButton.onClick.AddListener(OnSendReportClicked);
            _resetAllButton.onClick.AddListener(OnResetAllClicked);
        }

        private void OnDestroy()
        {
            _sendReportButton.onClick.RemoveListener(OnSendReportClicked);
            _resetAllButton.onClick.RemoveListener(OnResetAllClicked);
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Réinitialise tous les contrôleurs d’input.
        /// </summary>
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
        /// Envoie le rapport : pousse éventuellement des données (graph) puis affiche le popup de résultats.
        /// </summary>
        private void OnSendReportClicked()
        {
            _humidityRateController.SendDatasToGraph(_humidityRateController.CurrentHumidityPercent);
            ShowResultsPopup();
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
                    GenerateReportElements(popup, popup.BodyParentContent);
                    popup.RefreshLayouts();
                }
            };

            popup.StartLoading(() => DataStatus.Success);
        }

        /// <summary>
        /// Construit les lignes du récap : eau / air / pression / vent / humidité + total.
        /// </summary>
        private void GenerateReportElements(UI_ReportDatasPopup popup, RectTransform parent)
        {
            if (WeatherHandlerData.CurrentWeather == null)
            {
                CreateReportElement(parent, "Weather not available", "0", Color.gray);
                return;
            }

            CalculateMoney(
                out float total,
                out float waterTempMoney,
                out float airTempMoney,
                out float airPressureMoney,
                out float windSpeedMoney,
                out float windOrientationMoney,
                out float humidityMoney
            );

            // Lignes détaillées
            var lines = new (string label, float amount)[]
            {
                ("Water temperature",  waterTempMoney),
                ("Air temperature",    airTempMoney),
                ("Air pressure",       airPressureMoney),
                ("Wind speed",         windSpeedMoney),
                ("Wind direction",     windOrientationMoney),
                ("Humidity",           humidityMoney),
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

            // Total
            CreateReportElement(
                parent,
                description: "Total",
                amount: $"{WeatherMoneyCalculator.FormatMoney(total)}$",
                color: WeatherMoneyCalculator.ColorForAmount(total)
            );

            // Applique le gain
            PlayerCurrency.Add(total);
        }

        /// <summary>
        /// Instancie un élément de ligne de rapport.
        /// </summary>
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
        private void CalculateMoney(
            out float total,
            out float waterTempMoney,
            out float airTempMoney,
            out float airPressureMoney,
            out float windSpeedMoney,
            out float windOrientationMoney,
            out float humidityMoney)
        {
            var w = WeatherHandlerData.CurrentWeather;

            waterTempMoney = WeatherMoneyCalculator.CalculateWaterTemperature(w.WaterTemperature, _waterTemperatureController.CurrentTemperature, _weatherMoneyResult);
            airTempMoney = WeatherMoneyCalculator.CalculateAirTemperature(w.AirTemperature, _airTemperatureController.CurrentTemperature, _weatherMoneyResult);
            airPressureMoney = WeatherMoneyCalculator.CalculateAirPressure(w.AtmosphericPressure, _airPressureController.CurrentRange, _weatherMoneyResult);
            windSpeedMoney = WeatherMoneyCalculator.CalculateWindSpeed(w.WindSpeed, _windController.CurrentWindSpeed, _weatherMoneyResult);
            windOrientationMoney = WeatherMoneyCalculator.CalculateWindDirection(w.WindOrientationType, _windController.CompassController.CurrentSelectedOrientation, _weatherMoneyResult);
            humidityMoney = WeatherMoneyCalculator.CalculateHumidity(w.Humidity, _humidityRateController.CurrentHumidityPercent, _weatherMoneyResult);

            total = waterTempMoney + airTempMoney + airPressureMoney + windSpeedMoney + windOrientationMoney + humidityMoney;

            Debug.Log(
                $"[Weather $] Water={waterTempMoney} | Air={airTempMoney} | Pressure={airPressureMoney} | " +
                $"WindSpd={windSpeedMoney} | WindDir={windOrientationMoney} | Hum={humidityMoney} | TOTAL={total}"
            );
        }

        #endregion
    }
}
