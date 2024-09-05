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

public enum WindDirection
{
    North,
    East,
    South,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest
}

[System.Serializable]
public struct DayWeather
{
    public float humidity;
    public float windSpeed;
    public float airTemperature;
    public float windOrientationValue;
    public WindDirection windDirection;
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
    [SerializeField] private List<WeatherPreset> _weatherPresets;

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

    [Header("Wind")]
    public float MinAtmosphericPressure = 950f;
    public float MaxAtmosphericPressure = 1100f;

    [Header("DEBUGS INFOS --- ONLY")]
    [SerializeField] public List<DayWeather> weatherForecast;
    [SerializeField] private WeatherType _currentWeatherType;
    [SerializeField] private int indexWeather = 0;

    public DayWeather currentWeather;
    public DayWeather nextWeather;

    public float Humidity;
    public float WindSpeed;
    public float WindOrientationValue;
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
            weatherChangeDuration = currentWeather.weatherDuration;
            _onWeatherOverrideStart?.Raise(currentWeather.weatherType);
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
            weatherChangeDuration = nextWeather.weatherDuration;

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

        float totalWeight = _weatherPattern.StormyWeight + _weatherPattern.SunnyWeight + _weatherPattern.RainyWeight + _weatherPattern.WindyWeight + _weatherPattern.CalmyWeight;

        int stormsCount = Mathf.Clamp((int)((_weatherPattern.StormyWeight / totalWeight) * _weatherDurations.Count), _weatherPattern.MinStormWeathers, _weatherPattern.MaxStormWeathers);
        int sunnysCount = Mathf.Clamp((int)((_weatherPattern.SunnyWeight / totalWeight) * _weatherDurations.Count), _weatherPattern.MinSunnyWeathers, _weatherPattern.MaxSunnyWeathers);
        int rainysCount = Mathf.Clamp((int)((_weatherPattern.RainyWeight / totalWeight) * _weatherDurations.Count), _weatherPattern.MinRainyWeathers, _weatherPattern.MaxRainyWeathers);
        int windysCount = Mathf.Clamp((int)((_weatherPattern.WindyWeight / totalWeight) * _weatherDurations.Count), _weatherPattern.MinWindyWeathers, _weatherPattern.MaxWindyWeathers);
        int calmysCount = Mathf.Clamp((int)((_weatherPattern.CalmyWeight / totalWeight) * _weatherDurations.Count), _weatherPattern.MinCalmyWeathers, _weatherPattern.MaxCalmyWeathers);

        // Liste pour les types de météo en fonction des jours calculés
        List<WeatherType> weatherTypes = new List<WeatherType>();

        AddWeatherType(weatherTypes, WeatherType.Storm, stormsCount);
        AddWeatherType(weatherTypes, WeatherType.Sunny, sunnysCount);
        AddWeatherType(weatherTypes, WeatherType.Rainy, rainysCount);
        AddWeatherType(weatherTypes, WeatherType.Windy, windysCount);
        AddWeatherType(weatherTypes, WeatherType.Calm, calmysCount);

        // Vérification : s'il manque des types de météo pour correspondre au nombre de jours
        int totalWeathers = weatherTypes.Count;
        if (totalWeathers < _weatherDurations.Count)
        {
            int diff = _weatherDurations.Count - totalWeathers;
            // Ajoute des types de météo supplémentaires (par exemple, "Calm") pour combler l'écart
            AddWeatherType(weatherTypes, WeatherType.Calm, diff);
        }

        // Mélange aléatoire des types de météo
        weatherTypes.Shuffle();

        // Générer les paramètres météo pour chaque jour
        for (int i = 0; i < _weatherDurations.Count; i++)
        {
            DayWeather dayWeather = new DayWeather();
            dayWeather.weatherType = weatherTypes[i];
            dayWeather.weatherDuration = _weatherDurations[i];

            WeatherPreset preset = GetWeatherPresetForType(dayWeather.weatherType);

            dayWeather.windSpeed = Random.Range(preset.minWindSpeed, preset.maxWindSpeed) * difficulty;
            dayWeather.humidity = Random.Range(preset.minHumidity, preset.maxHumidity);
            dayWeather.airTemperature = Random.Range(preset.minAirTemperature, preset.maxAirTemperature);
            dayWeather.waterTemperature = Random.Range(preset.minWaterTemperature, preset.maxWaterTemperature);
            dayWeather.atmosphericPressure = Random.Range(preset.minAtmosphericPressure, preset.maxAtmosphericPressure);

            dayWeather.windOrientationValue = Random.Range(0f, 360f);
            dayWeather.windDirection = DetermineWindDirection(dayWeather.windOrientationValue);

            weatherForecast.Add(dayWeather);
        }
    }

    private WeatherPreset GetWeatherPresetForType(WeatherType weatherType)
    {
        return _weatherPresets.Find(x => x.weatherType == weatherType);
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
    public WeatherType DetermineWeatherType(DayWeather dayWeather)
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

    public WindDirection DetermineWindDirection(float windOrientation)
    {
        if (windOrientation >= 337.5f || windOrientation < 22.5f)
            return WindDirection.North;
        else if (windOrientation >= 22.5f && windOrientation < 67.5f)
            return WindDirection.NorthEast;
        else if (windOrientation >= 67.5f && windOrientation < 112.5f)
            return WindDirection.East;
        else if (windOrientation >= 112.5f && windOrientation < 157.5f)
            return WindDirection.SouthEast;
        else if (windOrientation >= 157.5f && windOrientation < 202.5f)
            return WindDirection.South;
        else if (windOrientation >= 202.5f && windOrientation < 247.5f)
            return WindDirection.SouthWest;
        else if (windOrientation >= 247.5f && windOrientation < 292.5f)
            return WindDirection.West;
        else
            return WindDirection.NorthWest;
    }


    // Mettre à jour la météo d'aujourd'hui et de demain
    private void UpdateTodayAndTomorrowWeather()
    {
        if (indexWeather < weatherForecast.Count)
        {
            currentWeather = weatherForecast[indexWeather];
            if (indexWeather + 1 < weatherForecast.Count)
                nextWeather = weatherForecast[indexWeather + 1];
            else
                nextWeather = currentWeather; // Si nous sommes au dernier jour, demain sera identique à aujourd'hui
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

        Humidity = Mathf.Lerp(currentWeather.humidity, nextWeather.humidity, lerpFactor);
        WindSpeed = Mathf.Lerp(currentWeather.windSpeed, nextWeather.windSpeed, lerpFactor);
        WindOrientationValue = Mathf.Lerp(currentWeather.windOrientationValue, nextWeather.windOrientationValue, lerpFactor);
        AirTemperature = Mathf.Lerp(currentWeather.airTemperature, nextWeather.airTemperature, lerpFactor);
        WaterTemperature = Mathf.Lerp(currentWeather.waterTemperature, nextWeather.waterTemperature, lerpFactor);
        AtmosphericPressure = Mathf.Lerp(currentWeather.atmosphericPressure, nextWeather.atmosphericPressure, lerpFactor);
    }

    // Appliquer les effets de la météo en fonction du type de météo actuel
    private void ApplyWeatherEffects()
    {
        _currentWeatherType = currentWeather.weatherType;

        // Vous pouvez ajouter des effets visuels/sonores ici en fonction du type de météo
        switch (_currentWeatherType)
        {
            case WeatherType.Storm:
                Debug.Log("Tempête en cours !");
                // Ajouter des effets de tempête, sons, etc.
                _onWeatherChanged?.Raise(WeatherType.Storm);
                _onWeatherOverrideStart?.Raise(WeatherType.Storm);
                break;
            case WeatherType.Windy:
                Debug.Log("Journée venteuse.");
                // Ajouter des effets de vent fort
                _onWeatherChanged?.Raise(WeatherType.Windy);
                _onWeatherOverrideStart?.Raise(WeatherType.Windy);
                break;
            case WeatherType.Rainy:
                Debug.Log("Il pleut.");
                _onWeatherChanged?.Raise(WeatherType.Rainy);
                _onWeatherOverrideStart?.Raise(WeatherType.Rainy);
                // Ajouter des effets de pluie
                break;
            case WeatherType.Sunny:
                Debug.Log("Journée ensoleillée.");
                // Ajouter des effets de beau temps
                _onWeatherChanged?.Raise(WeatherType.Sunny);
                _onWeatherOverrideStart?.Raise(WeatherType.Sunny);
                break;
            case WeatherType.Calm:
                Debug.Log("Eau calme.");
                // Ajouter des effets d'eau calme
                _onWeatherChanged?.Raise(WeatherType.Calm);
                _onWeatherOverrideStart?.Raise(WeatherType.Calm);
                break;
        }
    }
}
