using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeatherDatabase", menuName = "LightHouse/Weather/Database")]
public class WeatherDefinitionDatabase : ScriptableObject
{
    public List<WeatherDefinition> Definitions;

    public WeatherDefinition GetDefinition(WeatherType type)
    {
        return Definitions.Find(d => d.Type == type);
    }
}
