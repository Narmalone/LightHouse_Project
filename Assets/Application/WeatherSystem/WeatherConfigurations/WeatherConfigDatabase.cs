using System.Collections.Generic;
using UnityEngine;


namespace LightHouse.Weather
{
    [CreateAssetMenu(fileName = "WeatherDatabase", menuName = "LightHouse/Weather/Database")]
    public class WeatherConfigDatabase : ScriptableObject
    {
        public List<WeatherConfiguration> Definitions;

        public WeatherConfiguration GetDefinition(WeatherType type)
        {
            return Definitions.Find(d => d.Type == type);
        }
    }
}
