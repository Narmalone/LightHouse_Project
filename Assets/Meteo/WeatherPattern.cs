using UnityEngine;

[CreateAssetMenu(fileName = "WeatherPattern", menuName = "Meteo/Weather Pattern")]
public class WeatherPattern : ScriptableObject
{
    [Header("Tempête")]
    public int minStormDays = 5;
    public int maxStormDays = 7;

    [Header("Journées ensoleillées")]
    public int minSunnyDays = 10;
    public int maxSunnyDays = 15;

    [Header("Journées de pluie")]
    public int minRainyDays = 4;
    public int maxRainyDays = 7;

    [Header("Journées venteuses")]
    public int minWindyDays = 3;
    public int maxWindyDays = 6;

    [Header("Journées calmes")]
    public int minCalmDays = 5;
    public int maxCalmDays = 8;

    // Vous pouvez ajouter d'autres types de météo et ajuster les min/max ici.
}
