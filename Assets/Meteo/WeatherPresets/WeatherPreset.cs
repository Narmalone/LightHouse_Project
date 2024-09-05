using UnityEngine;

[CreateAssetMenu(fileName = "WeatherPreset", menuName = "Weather/WeatherPreset", order = 1)]
public class WeatherPreset : ScriptableObject
{
    public WeatherType weatherType;

    [Header("Temperature (°C)")]
    public float minAirTemperature;
    public float maxAirTemperature;

    [Header("Water Temperature (°C)")]
    public float minWaterTemperature;
    public float maxWaterTemperature;

    [Header("Humidity (%)")]
    public float minHumidity;
    public float maxHumidity;

    [Header("Atmospheric Pressure (hPa)")]
    public float minAtmosphericPressure;
    public float maxAtmosphericPressure;

    [Header("Wind Speed (km/h)")]
    public float minWindSpeed;
    public float maxWindSpeed;
}