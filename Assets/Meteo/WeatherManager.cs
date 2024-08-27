using System;
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

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;

    [Header("WEATHER SETTINGS")]
    [SerializeField] private WeatherPattern _weatherPattern;
    // Variable de difficulté
    [Range(0f, 2f)]
    public float difficulty = 1.0f;

    public bool FullRandomWeather = false;

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

    private GameManager _gm;

    private void Start()
    {
        _gm = GameManager.Instance;
        if (FullRandomWeather)
        {
            GenerateRandomWeatherForecast();
        }
        else
        {
            GenerateWeatherForecast();
        }

        // Charger la météo du premier jour et du lendemain
        UpdateTodayAndTomorrowWeather();

        weatherChangeDuration = todayWeather.weatherDuration;

        CalculateHowManyMeteoForDays(_gm.gameSettings.DayCycleDuration, _gm.gameSettings.TotalDays);

    }

    private void Update()
    {
        // Mettre à jour le temps écoulé
        elapsedTime += Time.deltaTime;

        // Vérifier si le temps écoulé a dépassé la durée aléatoire
        if (elapsedTime >= weatherChangeDuration)
        {
            // Réinitialiser le temps écoulé et choisir une nouvelle durée aléatoire
            elapsedTime = 0f;
            weatherChangeDuration = tomorrowWeather.weatherDuration;

            // Changer la météo
            AdvanceToNextWeather();
        }

        // Interpolation des conditions météorologiques
        InterpolateWeatherConditions();

        // Simulation de la météo actuelle
        //ApplyWeatherEffects();
    }

    private int CalculateHowManyMeteoForDays(TimeDatas datas, int totalDay)
    {
        Debug.Log(datas.Duration);
        for (int i = 0; i < totalDay; i++)
        {

        }
        //TimeSpan tmp = TimeSpan.FromSeconds();
        return 0;
    }

    // Fonction utilitaire pour ajouter un certain nombre de jours d'un type de météo donné
    private void AddWeatherType(List<WeatherType> list, WeatherType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            list.Add(type);
        }
    }

    private void GenerateWeatherForecast()
    {
        weatherForecast = new List<DayWeather>();

        // Calculer combien de jours de chaque type de météo il doit y avoir
        int totalDays = _gm.gameSettings.TotalDays;

        int stormDays = Random.Range(_weatherPattern.minStormDays, _weatherPattern.maxStormDays + 1);
        int sunnyDays = Random.Range(_weatherPattern.minSunnyDays, _weatherPattern.maxSunnyDays + 1);
        int rainyDays = Random.Range(_weatherPattern.minRainyDays, _weatherPattern.maxRainyDays + 1);
        int windyDays = Random.Range(_weatherPattern.minWindyDays, _weatherPattern.maxWindyDays + 1);
        int calmDays = totalDays - stormDays - sunnyDays - rainyDays - windyDays;

        calmDays = Mathf.Clamp(calmDays, _weatherPattern.minCalmDays, _weatherPattern.maxCalmDays);

        // Liste pour les types de météo en fonction des jours calculés
        List<WeatherType> weatherTypes = new List<WeatherType>();

        AddWeatherType(weatherTypes, WeatherType.Storm, stormDays);
        AddWeatherType(weatherTypes, WeatherType.Sunny, sunnyDays);
        AddWeatherType(weatherTypes, WeatherType.Rainy, rainyDays);
        AddWeatherType(weatherTypes, WeatherType.Windy, windyDays);
        AddWeatherType(weatherTypes, WeatherType.Calm, calmDays);

        weatherTypes.Shuffle();

        // Générer les paramètres météo pour chaque jour en fonction du type de météo choisi
        for (int i = 0; i < totalDays; i++)
        {
            DayWeather dayWeather = new DayWeather();
            dayWeather.weatherType = weatherTypes[i];
            dayWeather.weatherDuration = Random.Range(MinWeatherDuration, MaxWeatherDuration);

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
        weatherForecast = new List<DayWeather>();

        // Initialisation pour des facteurs saisonniers (variation progressive)
        float baseAirTemp = Random.Range(10f, 25f); // Température moyenne de départ (peut changer selon la saison)
        float baseWaterTemp = Random.Range(10f, 20f); // Température de l'eau moyenne initiale
        float baseHumidity = Random.Range(50f, 70f); // Humidité moyenne de départ
        float basePressure = 1013f; // Pression atmosphérique normale au niveau de la mer

        for (int i = 0; i < 31; i++)
        {
            DayWeather dayWeather = new DayWeather();

            // Variations légères sur la base des jours précédents pour continuité
            dayWeather.airTemperature = Mathf.Clamp(baseAirTemp + Random.Range(-5f, 5f), -15f, 40f);
            dayWeather.waterTemperature = Mathf.Clamp(baseWaterTemp + Random.Range(-2f, 2f), 0f, 30f);
            dayWeather.humidity = Mathf.Clamp(baseHumidity + Random.Range(-10f, 10f), 20f, 100f);

            // Facteur réaliste de pression atmosphérique
            dayWeather.atmosphericPressure = Mathf.Clamp(basePressure + Random.Range(-15f, 15f), 950f, 1050f);

            // Influence des conditions atmosphériques sur la vitesse du vent
            if (dayWeather.atmosphericPressure < 1000f) // Baisse de pression, vents plus forts
            {
                dayWeather.windSpeed = Mathf.Clamp(Random.Range(MinWindSpeed * 1.5f, MaxWindSpeed * 0.9f) * difficulty, MinWindSpeed, MaxWindSpeed);
            }
            else // Pression plus élevée, vents plus calmes
            {
                dayWeather.windSpeed = Mathf.Clamp(Random.Range(MinWindSpeed, MaxWindSpeed * 0.5f) * difficulty, MinWindSpeed, MaxWindSpeed);
            }

            // Influence des températures sur la pression pour donner de la continuité
            baseAirTemp = dayWeather.airTemperature + Random.Range(-2f, 2f); // Changement progressif
            baseWaterTemp = dayWeather.waterTemperature + Random.Range(-1f, 1f);
            baseHumidity = dayWeather.humidity + Random.Range(-5f, 5f);
            basePressure = dayWeather.atmosphericPressure + Random.Range(-5f, 5f); // Pression change un peu chaque jour

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
