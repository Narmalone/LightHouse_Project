using UnityEngine;

namespace LightHouse.Features.Weather
{
    [CreateAssetMenu(fileName = "SO_WheatherConfig_Default", menuName = "LightHouse/Computer/LEO/Weather/New Config")]
    public class SO_WeatherConfigurations : ScriptableObject
    {
        public float StartHour = 9;
        public float EndHour = 12;
    }

}
