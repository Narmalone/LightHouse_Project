using UnityEngine;
using LightHouse.Game.DayNightSystem;

public class WeatherManager : MonoBehaviour
{
    public WeatherGenerator WeatherGenerator;
    public WeatherTimeline WeatherTime;
    public TimeConfiguration TimeConfig;
    public TimeManager TimeManager;

    public WeatherData CurrentWeather;
    private WeatherData fromWeather;
    private WeatherData toWeather;

    public WeatherData FromWeather => fromWeather;
    public WeatherData ToWeather => toWeather;

    private float currentGameSeconds;
    private float fromStart;
    public float CurrentBlend => Mathf.Clamp01((currentGameSeconds - fromStart) / fromWeather.DurationInSeconds);

    private int currentIndex = 0;
    private void Awake()
    {
        WeatherGenerator.FillTimeline(WeatherGenerator.MinWeathersDuration, WeatherGenerator.MaxWeathersDuration);
    }

    private void Start()
    {
        if (WeatherTime.weathers.Count < 2)
        {
            Debug.LogError("Pas assez d'événements météo pour interpoler !");
            return;
        }

        // Initialisation
        fromWeather = WeatherTime.weathers[0];
        toWeather = WeatherTime.weathers[1];
    }

    private void Update()
    {
        float secondsPerDay = TimeConfig.GetTotalSecondsPerDay();
        currentGameSeconds = TimeManager.currentDay * secondsPerDay + (TimeManager.currentTime / 24f) * secondsPerDay;

        fromStart = fromWeather.StartTimeInSeconds;
        float fromEnd = fromStart + fromWeather.DurationInSeconds;

        // Passe à la météo suivante si on a dépassé la durée
        if (currentGameSeconds > fromEnd && currentIndex < WeatherTime.weathers.Count - 2)
        {
            currentIndex++;
            fromWeather = WeatherTime.weathers[currentIndex];
            toWeather = WeatherTime.weathers[currentIndex + 1];
        }

        // Interpolation
        float localTime = currentGameSeconds - fromWeather.StartTimeInSeconds;
        float t = Mathf.Clamp01(localTime / fromWeather.DurationInSeconds);
        CurrentWeather = WeatherUtils.LerpWeatherData(fromWeather, toWeather, t);
    }
}
