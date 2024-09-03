using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum WeatherType
{
    Calm,   // Eau calme
    Storm,  // Tempête
    Windy,  // Vent fort
    Rainy,  // Pluie
    Sunny   // Soleil
}

[System.Serializable]
public struct DayWeather
{
    public float humidity;
    public float windSpeed;
    public float airTemperature;
    public float waterTemperature;
    public float atmosphericPressure;
    public float weatherDuration;
    public WeatherType weatherType; // Type de météo
}

public class WeatherManager : Singleton<WeatherManager>
{
    [Header("CONTROLLERS")]
    [SerializeField] private RainController _rainController;
    [SerializeField] private CloudsController _cloudsController;
    [SerializeField] private OceanController _oceanController;
    [SerializeField] private LightningsController _lightningController;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;
    [SerializeField] private CustomEvent_WeatherType _onWeatherOverrideStart;

    [Header("WEATHER SETTINGS")]
    [SerializeField] private WeatherPattern _weatherPattern;
    // Variable de difficulté
    [Range(0f, 2f)]
    public float difficulty = 1.0f;

    public bool FullRandomWeather = false;
    public bool GenerateWeatherByWeather = false;

    [Header("Duration")]
    public float MinWeatherDuration = 25f;
    public float MaxWeatherDuration = 150f;

    [Header("Wind")]
    public float MinWindSpeed = 5f;
    public float MaxWindSpeed = 100f;

    [Header("DEBUGS INFOS --- ONLY")]
    [SerializeField] private List<DayWeather> weatherForecast;
    [SerializeField] private WeatherType _currentWeatherType;
    [SerializeField] private int indexWeather = 0;

    public DayWeather todayWeather;
    public DayWeather tomorrowWeather;

    public float Humidity;
    public float WindSpeed;
    public float AirTemperature;
    public float WaterTemperature;
    public float AtmosphericPressure;

    private float elapsedTime = 0f;
    private float weatherChangeDuration;

    [SerializeField] private bool _weatherLoaded = false;
    private float _totalWeatherDuration = 0f;
    [SerializeField] private float _targetWeatherDuration = 0f;
    private List<float> _weatherDurations = new List<float>();

    private GameManager _gm;

    private void Start()
    {
        _gm = GameManager.Instance;

        _totalWeatherDuration = GetTotalDuration(_gm.gameSettings.DayCycleDuration, _gm.gameSettings.TotalDays);

        if (FullRandomWeather)
        {
            GenerateRandomWeatherForecast();
            UpdateTodayAndTomorrowWeather();
            weatherChangeDuration = todayWeather.weatherDuration;
            _onWeatherOverrideStart?.Raise(todayWeather.weatherType);
            _weatherLoaded = true;
        }
        else
        {
            StartCoroutine(RoutineGenerate());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            elapsedTime = weatherChangeDuration;
        }

        if (!_weatherLoaded) return;
        elapsedTime += Time.deltaTime;

        // Vérifier si le temps écoulé a dépassé la durée aléatoire
        if (elapsedTime >= weatherChangeDuration)
        {
            elapsedTime = 0f;
            weatherChangeDuration = tomorrowWeather.weatherDuration;

            // Changer la météo
            AdvanceToNextWeather();
        }

        InterpolateWeatherConditions();
    }

    private int GetTotalDuration(TimeDatas datas, int totalDay)
    {
        TimeSpan duration = new TimeSpan(
        (int)datas.Hour, // hours
        (int)datas.Minutes, // minutes
        (int)datas.Seconds // seconds
        );

        int calculation = 0;
        for (int i = 0; i < totalDay; i++)
        {
            calculation += (int)duration.TotalSeconds;
        }
        return calculation;
    }

    // Fonction utilitaire pour ajouter un certain nombre de jours d'un type de météo donné
    private void AddWeatherType(List<WeatherType> list, WeatherType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            list.Add(type);
        }
    }

    IEnumerator RoutineGenerate()
    {
        while (_targetWeatherDuration < _totalWeatherDuration)
        {
            float remainingDuration = _totalWeatherDuration - _targetWeatherDuration;
            float weatherDuration = Mathf.Min(remainingDuration, Random.Range(MinWeatherDuration, MaxWeatherDuration));
            _targetWeatherDuration += weatherDuration;
            _weatherDurations.Add(weatherDuration);
            yield return null;
        }
        
        GenerateWeatherForecast();
        weatherChangeDuration = _weatherDurations[0];
        UpdateTodayAndTomorrowWeather();
        _weatherLoaded = true;
    }

    private void GenerateWeatherForecast()
    {
        weatherForecast = new List<DayWeather>();

        // Calculate the total weight of all weather types
        float totalWeight = _weatherPattern.StormyWeight + _weatherPattern.SunnyWeight + _weatherPattern.RainyWeight + _weatherPattern.WindyWeight + _weatherPattern.CalmyWeight;

        // Calculate the number of days for each weather type based on their weights
        int stormsCount = (int)((_weatherPattern.StormyWeight / totalWeight) * _weatherDurations.Count);
        int sunnysCount = (int)((_weatherPattern.SunnyWeight / totalWeight) * _weatherDurations.Count);
        int rainysCount = (int)((_weatherPattern.RainyWeight / totalWeight) * _weatherDurations.Count);
        int windysCount = (int)((_weatherPattern.WindyWeight / totalWeight) * _weatherDurations.Count);
        int calmysCount = (int)((_weatherPattern.CalmyWeight / totalWeight) * _weatherDurations.Count);

        // Ensure the number of days for each weather type is within the min/max range
        stormsCount = Mathf.Clamp(stormsCount, _weatherPattern.MinStormWeathers, _weatherPattern.MaxStormWeathers);
        sunnysCount = Mathf.Clamp(sunnysCount, _weatherPattern.MinSunnyWeathers, _weatherPattern.MaxSunnyWeathers);
        rainysCount = Mathf.Clamp(rainysCount, _weatherPattern.MinRainyWeathers, _weatherPattern.MaxRainyWeathers);
        windysCount = Mathf.Clamp(windysCount, _weatherPattern.MinWindyWeathers, _weatherPattern.MaxWindyWeathers);
        calmysCount = Mathf.Clamp(calmysCount, _weatherPattern.MinCalmyWeathers, _weatherPattern.MaxCalmyWeathers);

        int totalWeathers = stormsCount + sunnysCount + rainysCount + windysCount + calmysCount;

        //securité
        if(totalWeathers < _weatherDurations.Count)
        {
            int diff = _weatherDurations.Count - totalWeathers;
            calmysCount += diff;
        }
            
        // Liste pour les types de météo en fonction des jours calculés
        List<WeatherType> weatherTypes = new List<WeatherType>();

        AddWeatherType(weatherTypes, WeatherType.Storm, stormsCount);
        AddWeatherType(weatherTypes, WeatherType.Sunny, sunnysCount);
        AddWeatherType(weatherTypes, WeatherType.Rainy, rainysCount);
        AddWeatherType(weatherTypes, WeatherType.Windy, windysCount);
        AddWeatherType(weatherTypes, WeatherType.Calm, calmysCount);

        weatherTypes.Shuffle();

        // Générer les paramètres météo pour chaque jour en fonction du type de météo choisi
        for (int i = 0; i < _weatherDurations.Count; i++)
        {
            DayWeather dayWeather = new DayWeather();
            dayWeather.weatherType = weatherTypes[i];
            dayWeather.weatherDuration = _weatherDurations[i];

            // Générer des valeurs basées sur le type de météo
            switch (dayWeather.weatherType)
            {
                case WeatherType.Storm:
                    dayWeather.windSpeed = Random.Range(80f, MaxWindSpeed);
                    dayWeather.humidity = Random.Range(70f, 100f);
                    dayWeather.airTemperature = Random.Range(10f, 25f);
                    dayWeather.atmosphericPressure = Random.Range(950f, 990f);
                    break;
                case WeatherType.Sunny:
                    dayWeather.windSpeed = Random.Range(MinWindSpeed, 20f);
                    dayWeather.humidity = Random.Range(30f, 50f);
                    dayWeather.airTemperature = Random.Range(20f, 35f);
                    dayWeather.atmosphericPressure = Random.Range(1010f, 1050f);
                    break;
                case WeatherType.Rainy:
                    dayWeather.windSpeed = Random.Range(MinWindSpeed, 50f);
                    dayWeather.humidity = Random.Range(80f, 100f);
                    dayWeather.airTemperature = Random.Range(10f, 20f);
                    dayWeather.atmosphericPressure = Random.Range(970f, 1005f);
                    break;
                case WeatherType.Windy:
                    dayWeather.windSpeed = Random.Range(50f, 80f);
                    dayWeather.humidity = Random.Range(50f, 70f);
                    dayWeather.airTemperature = Random.Range(15f, 25f);
                    dayWeather.atmosphericPressure = Random.Range(1000f, 1020f);
                    break;
                case WeatherType.Calm:
                    dayWeather.windSpeed = Random.Range(MinWindSpeed, 15f);
                    dayWeather.humidity = Random.Range(40f, 60f);
                    dayWeather.airTemperature = Random.Range(15f, 25f);
                    dayWeather.atmosphericPressure = Random.Range(1005f, 1025f);
                    break;
            }

            weatherForecast.Add(dayWeather);
        }
    }

    private void GenerateRandomWeatherForecast()
    {
        for (int i = 0; i < 100; i++)
        {
            DayWeather dayWeather = new DayWeather();
            dayWeather.weatherDuration = Random.Range(MinWeatherDuration, MaxWeatherDuration);
            dayWeather.airTemperature = Random.Range(-15f, 40f);
            dayWeather.waterTemperature = Random.Range(0f, 30f);
            dayWeather.humidity = Random.Range(20f, 100f);
            dayWeather.atmosphericPressure = Random.Range(950f, 1050f);
            dayWeather.windSpeed = Random.Range(MinWindSpeed, MaxWindSpeed);

            // Déterminer le type de météo basé sur des conditions corrélées
            dayWeather.weatherType = DetermineWeatherType(dayWeather);

            // Ajouter à la liste des prévisions
            weatherForecast.Add(dayWeather);
        }
    }

    // Déterminer le type de météo pour un jour donné en complexifiant la logique
    private WeatherType DetermineWeatherType(DayWeather dayWeather)
    {
        if (dayWeather.windSpeed > 80f || (dayWeather.atmosphericPressure < 980f && dayWeather.windSpeed > 50f))
        {
            return WeatherType.Storm; // Tempête avec vent très fort et basse pression
        }
        else if (dayWeather.windSpeed > 50f)
        {
            return WeatherType.Windy; // Journée très venteuse mais sans tempête
        }
        else if (dayWeather.humidity > 85f && dayWeather.atmosphericPressure < 1000f)
        {
            return WeatherType.Rainy; // Humidité élevée et basse pression
        }
        else if (dayWeather.atmosphericPressure > 1020f && dayWeather.humidity < 50f)
        {
            return WeatherType.Sunny; // Haute pression et faible humidité = beau temps
        }
        else
        {
            return WeatherType.Calm; // Conditions modérées et stables
        }
    }


    // Mettre à jour la météo d'aujourd'hui et de demain
    private void UpdateTodayAndTomorrowWeather()
    {
        if (indexWeather < weatherForecast.Count)
        {
            todayWeather = weatherForecast[indexWeather];
            if (indexWeather + 1 < weatherForecast.Count)
                tomorrowWeather = weatherForecast[indexWeather + 1];
            else
                tomorrowWeather = todayWeather; // Si nous sommes au dernier jour, demain sera identique à aujourd'hui
            ApplyWeatherEffects();
        }
    }

    // Avancer au jour suivant
    private void AdvanceToNextWeather()
    {
        indexWeather++;
        if (indexWeather < weatherForecast.Count)
        {
            UpdateTodayAndTomorrowWeather();
            Debug.Log("Changement météo vers le jour suivant : " + indexWeather);
        }
        else
        {
            Debug.Log("Simulation des 31 jours terminée.");
        }
    }

    // Interpolation des conditions météorologiques entre aujourd'hui et demain
    private void InterpolateWeatherConditions()
    {
        float lerpFactor = elapsedTime / weatherChangeDuration;

        Humidity = Mathf.Lerp(todayWeather.humidity, tomorrowWeather.humidity, lerpFactor);
        WindSpeed = Mathf.Lerp(todayWeather.windSpeed, tomorrowWeather.windSpeed, lerpFactor);
        AirTemperature = Mathf.Lerp(todayWeather.airTemperature, tomorrowWeather.airTemperature, lerpFactor);
        WaterTemperature = Mathf.Lerp(todayWeather.waterTemperature, tomorrowWeather.waterTemperature, lerpFactor);
        AtmosphericPressure = Mathf.Lerp(todayWeather.atmosphericPressure, tomorrowWeather.atmosphericPressure, lerpFactor);
    }

    // Appliquer les effets de la météo en fonction du type de météo actuel
    private void ApplyWeatherEffects()
    {
        _currentWeatherType = todayWeather.weatherType;

        // Vous pouvez ajouter des effets visuels/sonores ici en fonction du type de météo
        switch (_currentWeatherType)
        {
            case WeatherType.Storm:
                Debug.Log("Tempête en cours !");
                // Ajouter des effets de tempête, sons, etc.
                _onWeatherChanged?.Raise(WeatherType.Storm);
                break;
            case WeatherType.Windy:
                Debug.Log("Journée venteuse.");
                // Ajouter des effets de vent fort
                _onWeatherChanged?.Raise(WeatherType.Windy);

                break;
            case WeatherType.Rainy:
                Debug.Log("Il pleut.");
                _onWeatherChanged?.Raise(WeatherType.Rainy);
                // Ajouter des effets de pluie
                break;
            case WeatherType.Sunny:
                Debug.Log("Journée ensoleillée.");
                // Ajouter des effets de beau temps
                _onWeatherChanged?.Raise(WeatherType.Sunny);
                break;
            case WeatherType.Calm:
                Debug.Log("Eau calme.");
                // Ajouter des effets d'eau calme
                _onWeatherChanged?.Raise(WeatherType.Calm);
                break;
        }
    }
}
