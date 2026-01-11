using LightHouse.EditorTools.SuperGameManager;
using UnityEngine;

namespace LightHouse.Features.Weather
{
    [CreateAssetMenu(fileName = "NewWeatherDefinition", menuName = "LightHouse/Weather/Definition")]
    public class WeatherConfiguration : ScriptableObject
    {
        [SgmExpose(label: "Type")]
        public WeatherType Type;
        [SgmExpose(label: "Dangerous Level")]
        public float DangerLevel = 1.0f;
        [SgmExpose(label: "Min Weather Duration")]
        public float MinWeatherDuration = 120.0f;
        [SgmExpose(label: "Max Weather Duration")]
        public float MaxWeatherDuration = 360.0f;

        [Header("Conditions")]
        [SgmExpose(label: "Humidity Range")]
        public Vector2 HumidityRange = new Vector2(60f, 100f);
        [SgmExpose(label: "Pressure Range")]
        public Vector2 PressureRange = new Vector2(980f, 1005f);
        [SgmExpose(label: "Wind Speed Range")]
        public Vector2 WindSpeedRange = new Vector2(10f, 25f);
        [SgmExpose(label: "Air Temperature Range")]
        public Vector2 AirTemperatureRange = new Vector2(5f, 16f);
        [SgmExpose(label: "Water Temperature Range")]
        public Vector2 WaterTemperatureRange = new Vector2(4f, 14f);
    }
}
