using System.Collections.Generic;
using UnityEngine;

// Enumération des types de météo
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
    public WeatherType weatherType; // Type de météo
}

public class WeatherManager : Singleton<WeatherManager>
{
    [SerializeField] private RainController _rainController;
    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;
    // Liste des prévisions sur 31 jours
    [SerializeField] private List<DayWeather> weatherForecast;
    [SerializeField] private WeatherType _currentWeatherType;

    // Index du jour actuel (de 0 à 30)
    [SerializeField] private int indexWeather = 0;

    // Variables pour le jour actuel et le jour suivant
    public DayWeather todayWeather;
    public DayWeather tomorrowWeather;

    // Variable de difficulté
    [Range(0f, 2f)]
    public float difficulty = 1.0f;

    // Paramètres d'environnement
    private float minWindSpeed = 5f;
    [SerializeField] public float maxWindSpeed = 100f;

    // Variables pour les conditions actuelles interpolées
    public float humidity;
    public float windSpeed;
    public float airTemperature;
    public float waterTemperature;
    public float atmosphericPressure;

    // Temps écoulé dans la journée et durée aléatoire avant le prochain changement météo
    private float elapsedTime = 0f;
    private float weatherChangeDuration;

    // Simulation du temps de la journée (maximum 24h par cycle)
    public float minDayDuration = 25f; // Durée maximale d'une journée en secondes (24 heures)
    public float maxDayDuration = 150f; // Durée maximale d'une journée en secondes (24 heures)

    private void Start()
    {
        // Générer les prévisions météo pour les 31 jours
        GenerateWeatherForecast();

        // Charger la météo du premier jour et du lendemain
        UpdateTodayAndTomorrowWeather();

        // Initialiser la première durée de changement météo
        SetNextWeatherChangeDuration();
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
            SetNextWeatherChangeDuration();

            // Changer la météo
            AdvanceToNextWeather();
        }

        // Interpolation des conditions météorologiques
        InterpolateWeatherConditions();

        // Simulation de la météo actuelle
        //ApplyWeatherEffects();
    }

    // Génération des prévisions météo sur 31 jours
    private void GenerateWeatherForecast()
    {
        weatherForecast = new List<DayWeather>();

        for (int i = 0; i < 31; i++)
        {
            DayWeather dayWeather = new DayWeather();

            // Générer des valeurs aléatoires pour les paramètres météorologiques
            dayWeather.humidity = Random.Range(30f, 100f);
            dayWeather.windSpeed = Random.Range(minWindSpeed, maxWindSpeed) * difficulty; // Facteur de difficulté
            dayWeather.airTemperature = Random.Range(-10f, 35f); // Plage de température réaliste
            dayWeather.waterTemperature = Random.Range(5f, 25f);
            dayWeather.atmosphericPressure = Random.Range(950f, 1050f); // Valeurs réalistes de pression atmosphérique

            // Déterminer le type de météo basé sur les paramètres
            dayWeather.weatherType = DetermineWeatherType(dayWeather);

            // Ajouter à la liste des prévisions
            weatherForecast.Add(dayWeather);
        }
    }

    // Déterminer le type de météo pour un jour donné
    private WeatherType DetermineWeatherType(DayWeather dayWeather)
    {
        if (dayWeather.windSpeed > 80f)
            return WeatherType.Storm;
        else if (dayWeather.windSpeed > 50f)
            return WeatherType.Windy;
        else if (dayWeather.humidity > 80f && dayWeather.airTemperature < 20f)
            return WeatherType.Rainy;
        else if (dayWeather.atmosphericPressure > 1015f)
            return WeatherType.Sunny;
        else
            return WeatherType.Calm;
    }

    // Mettre à jour la météo d'aujourd'hui et de demain
    private void UpdateTodayAndTomorrowWeather()
    {
        if (indexWeather < 31)
        {
            todayWeather = weatherForecast[indexWeather];
            if (indexWeather + 1 < 31)
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
        if (indexWeather < 31)
        {
            UpdateTodayAndTomorrowWeather();
            Debug.Log("Changement météo vers le jour suivant : " + indexWeather);
        }
        else
        {
            Debug.Log("Simulation des 31 jours terminée.");
        }
    }

    // Définir la durée aléatoire avant le prochain changement météo
    private void SetNextWeatherChangeDuration()
    {
        // Choisir une durée aléatoire entre quelques minutes et la durée maximale de la journée
        weatherChangeDuration = Random.Range(minDayDuration, maxDayDuration); // 300s = 5 minutes minimum
        Debug.Log("Durée avant le prochain changement météo : " + weatherChangeDuration + " secondes.");
    }

    // Interpolation des conditions météorologiques entre aujourd'hui et demain
    private void InterpolateWeatherConditions()
    {
        float lerpFactor = elapsedTime / weatherChangeDuration;

        humidity = Mathf.Lerp(todayWeather.humidity, tomorrowWeather.humidity, lerpFactor);
        windSpeed = Mathf.Lerp(todayWeather.windSpeed, tomorrowWeather.windSpeed, lerpFactor);
        airTemperature = Mathf.Lerp(todayWeather.airTemperature, tomorrowWeather.airTemperature, lerpFactor);
        waterTemperature = Mathf.Lerp(todayWeather.waterTemperature, tomorrowWeather.waterTemperature, lerpFactor);
        atmosphericPressure = Mathf.Lerp(todayWeather.atmosphericPressure, tomorrowWeather.atmosphericPressure, lerpFactor);
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
