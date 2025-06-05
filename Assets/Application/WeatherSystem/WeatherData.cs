using UnityEngine;

public enum WeatherType
{
    Sunny,
    Cloudy,
    Rainy,
    Windy,
    Foggy,
    Snowy,
    Stormy,
}

public enum WindOrientationType
{
    North,
    North_East,
    East,
    South_East,
    South,
    South_West,
    West,
    North_West
}

[System.Serializable]
public class WeatherData
{
    /// <summary>
    /// Automatically determined
    /// </summary>
    public WeatherType WeatherType;

    /// <summary>
    /// Automatically Determined by the <see cref="WindOrientation"/>
    /// </summary>
    public WindOrientationType WindOrientationType;

    /// <summary>
    /// The time whenn the meteo start
    /// </summary>
    public float StartTimeInSeconds;

    /// <summary>
    /// The total duration of this Meteo
    /// </summary>
    public float DurationInSeconds;

    // Données météorologiques continues
    /// <summary>
    /// The humidity value from 0 to 100
    /// </summary>
    public float Humidity;         // 0 à 100

    /// <summary>
    /// The Atmospheric pressure from
    /// </summary>
    public float AtmosphericPressure;// hPa
    public float WindSpeed;        // m/s
    public float WindOrientation; //360°

    public float WaterTemperature; //°C
    public float AirTemperature; //°C
}
