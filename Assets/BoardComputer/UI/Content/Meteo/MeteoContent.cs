using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeteoContent : ContentWindow
{
    [SerializeField] private TextMeshProUGUI _weatherReportTxt;
    [SerializeField] private CustomEvent_WindDirection _windInputDirection;
    public TemperatureController TemperatureController;
    public QualityAtmosphereController QualityAtmosphereController;
    public WindController WindController;

    public LanguageText[] AllFixedTexts;

    public WeatherData WeatherDataInput = new WeatherData(); //condensé de toutes les informations

    public Button SendDatasButton;

    private void Awake()
    {
        TemperatureController.AirTempInputField.onValueChanged.AddListener(OnAirTemperatureInputChanged);
        TemperatureController.WaterTempInputFIeld.onValueChanged.AddListener(OnWaterTemperatureInputChanged);

        QualityAtmosphereController.AtmosphericController.AtmosphericPressureIPF.onValueChanged.AddListener(OnAtmosphericInputChanged);
        QualityAtmosphereController.HumidityController.HumiditySlider.onValueChanged.AddListener(OnHumiditySliderChanged);

        _windInputDirection.handle += OnWindDirectionCliqued;
        WindController.BeaufortController.Slider.onValueChanged.AddListener(OnBeaufortSliderChanged);

        SendDatasButton.onClick.AddListener(OnSendDataCliqued);
    }

    private void OnDestroy()
    {
        TemperatureController.AirTempInputField.onValueChanged.RemoveListener(OnAirTemperatureInputChanged);
        TemperatureController.WaterTempInputFIeld.onValueChanged.RemoveListener(OnWaterTemperatureInputChanged);

        QualityAtmosphereController.AtmosphericController.AtmosphericPressureIPF.onValueChanged.RemoveListener(OnAtmosphericInputChanged);
        QualityAtmosphereController.HumidityController.HumiditySlider.onValueChanged.RemoveListener(OnHumiditySliderChanged);

        _windInputDirection.handle -= OnWindDirectionCliqued;
        WindController.BeaufortController.Slider.onValueChanged.RemoveListener(OnBeaufortSliderChanged);
        SendDatasButton.onClick.RemoveListener(OnSendDataCliqued);

    }

    //Déjà calculer la météo en fonction de l'heure à laquelle le joueur les rentre
    //également pour avoir quelque chose de faire play avoir un bouton valider pour chacuns des modules ?
    //ou bien alors le gros bouton valider
    //Mettre à jour les différents graphiques, échelle beaufort graph de pA, Bulletin prévisionnel.
    private void OnSendDataCliqued()
    {
        if (WeatherManager.Instance == null) return;
        float windSpeedProximity = WeatherDataInput.windSpeed.CalculateProximityPercent(WeatherManager.Instance.CurrentWeather.windSpeed);

        //plus tard changer le systeme par l'orientation directement (une aiguille qui tourne dans l'ui plutôt que pleins de vieilles
        //flèches.
        bool directionProxmity = WeatherDataInput.windDirection == WeatherManager.Instance.CurrentWeather.windDirection;

        float waterTempProximity = WeatherDataInput.waterTemperature.CalculateProximityPercent(WeatherManager.Instance.CurrentWeather.waterTemperature);
        float airTempProximity = WeatherDataInput.airTemperature.CalculateProximityPercent(WeatherManager.Instance.CurrentWeather.airTemperature);

        float humidityProximity = WeatherDataInput.humidity.CalculateProximityPercent(WeatherManager.Instance.CurrentWeather.humidity);

        float paProximity = WeatherDataInput.atmosphericPressure.CalculateProximityPercent(WeatherManager.Instance.CurrentWeather.atmosphericPressure);
    }

    private void OnBeaufortSliderChanged(float arg0)
    {
        WeatherDataInput.windSpeed = WindController.BeaufortController.SliderToBeaufort(arg0);
    }

    private void OnWindDirectionCliqued(WindDirection direction)
    {
        WeatherDataInput.windDirection = direction;
    }

    private void OnHumiditySliderChanged(float arg0)
    {
        WeatherDataInput.humidity = arg0;
    }

    private void OnAtmosphericInputChanged(string arg0)
    {
        if (int.TryParse(arg0, out int value))
        {
            WeatherDataInput.atmosphericPressure = value;
        }
    }

    private void OnWaterTemperatureInputChanged(string arg0)
    {
        if (float.TryParse(arg0, out float value))
        {
            WeatherDataInput.waterTemperature = value;
        }
    }

    private void OnAirTemperatureInputChanged(string arg0)
    {
        if(float.TryParse(arg0, out float value))
        {
            WeatherDataInput.airTemperature = value;
        }
    }

    
}
