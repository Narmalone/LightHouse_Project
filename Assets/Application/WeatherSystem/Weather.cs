using UnityEngine;

public enum WeatherType
{
    Sunny,
    Cloudy,
    Windy,
    Stormy,
}

[System.Serializable]
public class WeatherData
{
    public WeatherType WeatherType;
    //Données générales concernant la météo
    public float StartTimeInSeconds;
    public float DurationInSeconds;

    // Données météorologiques continues
    public float Humidity;         // 0 à 1
    public float Pressure;         // hPa
    public float Temperature;      // °C
    public float WindSpeed;        // m/s
}
