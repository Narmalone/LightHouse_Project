using UnityEngine;

[CreateAssetMenu(fileName = "WeatherData", menuName = "ScriptableObjects/WeatherData", order = 1)]
public class WeatherDataSO : ScriptableObject
{
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

    [Range(0f, 200f)] public float windSpeed;
    [Range(0f, 360f)] public float windDirection;
    [Range(0f, 1f)] public float turbulence;
    [Range(0f, 100f)] public float humidity;
}
