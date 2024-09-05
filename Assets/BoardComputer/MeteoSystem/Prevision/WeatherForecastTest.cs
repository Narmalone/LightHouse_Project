using System;
using UnityEngine;
using TMPro;

public class WeatherForecast : MonoBehaviour
{
    [Header("Weather Manager")]
    [SerializeField] private WeatherManager _weatherManager;
    [SerializeField] private DayNightManager _dayNightManager;

    [SerializeField] private CustomEvent_WeatherType _onWeatherGenerated;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI morningWeatherText;
    [SerializeField] private TextMeshProUGUI middayWeatherText;
    [SerializeField] private TextMeshProUGUI eveningWeatherText;

    [Header("Forecast Settings")]
    [SerializeField] private float morningStart = 6f;
    [SerializeField] private float middayStart = 12f;
    [SerializeField] private float eveningStart = 18f;

    private DayWeather morningWeather;
    private DayWeather middayWeather;
    private DayWeather eveningWeather;

    private void Awake()
    {
        _onWeatherGenerated.handle += _onWeatherGenerated_handle;
    }

    private void OnDestroy()
    {
        _onWeatherGenerated.handle -= _onWeatherGenerated_handle;
    }

    private void _onWeatherGenerated_handle(WeatherType obj)
    {
        
    }

    private void Start()
    {
        // Initialiser les prévisions au démarrage
        //UpdateWeatherForecast();
    }

    private void Update()
    {
        Debug.Log(_dayNightManager.TimeUntil(12f));
        // Mettre à jour les prévisions à chaque changement d'état du cycle jour/nuit
        if (_dayNightManager.State == DayNightManager.DayState.MORNING ||
            _dayNightManager.State == DayNightManager.DayState.MID_DAY ||
            _dayNightManager.State == DayNightManager.DayState.EVENING)
        {
            //UpdateWeatherForecast();
        }
    }

    private void UpdateWeatherForecast()
    {
        // Prévisions du matin, midi, et soir basées sur l'heure actuelle
        morningWeather = GetWeatherForTime(morningStart);
        middayWeather = GetWeatherForTime(middayStart);
        eveningWeather = GetWeatherForTime(eveningStart);

        // Afficher les prévisions à l'écran
        UpdateWeatherUI();
    }

    private DayWeather GetWeatherForTime(float time)
    {
        //pour calcul de la weather
        //déterminer en secondes dans combien de temps il sera midi, morning ect... par rapport au current time
        //à l'aide de ce rapport en secondes donc dans par exemple X s on détermine si par ex midi ce sera le "même temps ou le prochain"
        //si c'est le même ou par rapport à la prochaine on fais un calcul pour faire un "bond" dans le temps en mode
        //

        // Choisir le type de météo basé sur l'heure de la journée
        DayWeather forecastWeather = new DayWeather();
        float hour = time % 24f;

        // Obtenir la météo la plus proche pour la période donnée
        foreach (var dayWeather in _weatherManager.weatherForecast)
        {
            if (hour >= morningStart && hour < middayStart)
            {
                forecastWeather = _weatherManager.currentWeather;
            }
            else if (hour >= middayStart && hour < eveningStart)
            {
                forecastWeather = _weatherManager.currentWeather;
            }
            else if (hour >= eveningStart || hour < morningStart)
            {
                forecastWeather = _weatherManager.nextWeather;
            }
        }

        return forecastWeather;
    }

    private void UpdateWeatherUI()
    {
        // Afficher les prévisions dans l'UI
        morningWeatherText.text = FormatWeather(morningWeather, "Matin");
        middayWeatherText.text = FormatWeather(middayWeather, "Midi");
        eveningWeatherText.text = FormatWeather(eveningWeather, "Soir");
    }

    private string FormatWeather(DayWeather weather, string period)
    {
        // Formatage des données météo pour l'affichage
        return $"{period}: {weather.weatherType}\n" +
               $"Température de l'air: {weather.airTemperature}°C\n" +
               $"Température de l'eau: {weather.waterTemperature}°C\n" +
               $"Humidité: {weather.humidity}%\n" +
               $"Pression atmosphérique: {weather.atmosphericPressure} hPa\n" +
               $"Vitesse du vent: {weather.windSpeed} km/h\n" +
               $"Direction du vent: {weather.windDirection}";
    }
}
