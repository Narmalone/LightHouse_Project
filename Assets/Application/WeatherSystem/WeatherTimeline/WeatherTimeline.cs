using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Weather
{
    [CreateAssetMenu(fileName = "WheaterTimeline_", menuName = "LightHouse/WeatherSystem/New Timeline")]
    public class WeatherTimeline : ScriptableObject
    {
        public List<WeatherData> Weathers = new List<WeatherData>();
    }
}
