using UnityEngine;

[CreateAssetMenu(fileName = "NewWeatherDefinition", menuName = "LightHouse/Weather/Definition")]
public class WeatherDefinition : ScriptableObject
{
    public WeatherType Type;

    [Header("Conditions")]
    public Vector2 HumidityRange = new Vector2(60f, 100f);
    public Vector2 PressureRange = new Vector2(980f, 1005f);
    public Vector2 WindSpeedRange = new Vector2(10f, 25f);
    public Vector2 AirTemperatureRange = new Vector2(5f, 16f);
    public Vector2 WaterTemperatureRange = new Vector2(4f, 14f);
}
