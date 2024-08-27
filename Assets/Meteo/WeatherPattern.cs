using UnityEngine;

[CreateAssetMenu(fileName = "WeatherPattern", menuName = "Meteo/Weather Pattern")]
public class WeatherPattern : ScriptableObject
{
    [Header("Tempête")]
    [Range(0, 1)] public float StormyWeight = 0.1f;
    public int MinStormWeathers = 5;
    public int MaxStormWeathers = 7;

    [Header("Journées ensoleillées")]
    [Range(0, 1)] public float SunnyWeight = 0.3f;
    public int MinSunnyWeathers = 10;
    public int MaxSunnyWeathers = 15;

    [Header("Journées de pluie")]
    [Range(0, 1)] public float RainyWeight = 0.3f;
    public int MinRainyWeathers = 4;
    public int MaxRainyWeathers = 7;

    [Header("Journées venteuses")]
    [Range(0, 1)] public float WindyWeight = 0.3f;
    public int MinWindyWeathers = 3;
    public int MaxWindyWeathers = 6;

    [Header("Journées calmes")]
    [Range(0, 1)] public float CalmyWeight= 0.5f;
    public int MinCalmyWeathers = 5;
    public int MaxCalmyWeathers = 8;

    // Vous pouvez ajouter d'autres types de météo et ajuster les min/max ici.
}
