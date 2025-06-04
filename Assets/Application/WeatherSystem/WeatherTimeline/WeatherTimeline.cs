using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WheaterTimeline_", menuName = "Lighthouse/WeatherSystem/New Timeline")]
public class WeatherTimeline : ScriptableObject
{
    public List<WeatherData> weathers = new List<WeatherData>();
}
