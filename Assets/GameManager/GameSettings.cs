using System;
using UnityEngine;

//regroupera par la suite, options settings, difficultée settings ect..
[CreateAssetMenu(menuName = "GameSettings")]
public class GameSettings : ScriptableObject
{
    public int TotalDays = 14;
    public TimeDatas DayCycleDuration = new TimeDatas() { Hour = 0, Minutes = 4.5f, Seconds = 0f };
    public TimeDatas NightCycleDuration = new TimeDatas() { Hour = 0, Minutes = 4.5f, Seconds = 0f };

    [SerializeField] public ScenarioSettings scenarioSettings;
    
}

[System.Serializable]
public struct TimeDatas
{
    public float Hour;
    public float Minutes;
    public float Seconds;

    public TimeDatas(float hour, float minutes, float seconds)
    {
        Hour = hour;
        Minutes = minutes;
        Seconds = seconds;
    }
}
