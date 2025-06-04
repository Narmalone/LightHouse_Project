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
    /// <summary>
    /// Automatically determined
    /// </summary>
    public WeatherType WeatherType;
    //Données générales concernant la météo
    public float StartTimeInSeconds;
    public float DurationInSeconds;

    // Données météorologiques continues
    public float Humidity;         // 0 à 1
    public float Pressure;         // hPa
    public float WindSpeed;        // m/s
    public float WindOrientation; //360°

    public float WaterTemperature; //°C
    public float AirTemperature; //°C
}
