using UnityEngine;

[CreateAssetMenu(fileName = "NewDayNight", menuName = "Gameplay/DayNight/DayNightSettings")]
public class DayNightSettings : ScriptableObject
{
    [Range(0, 24)] public float MorningStartHour = 6;
    [Range(0, 24)] public float MiddayStartHour = 12;
    [Range(0, 24)] public float EveningStartHour = 18;
    [Range(0, 24)] public float NightStartHour = 21;
}
