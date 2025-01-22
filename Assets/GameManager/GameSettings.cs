using System;
using UnityEngine;

//regroupera par la suite, options settings, difficultée settings ect..
[CreateAssetMenu(menuName = "GameSettings")]
public class GameSettings : ScriptableObject
{
    public int TotalDays = 31;
    public TimeDatas DayCycleDuration = new TimeDatas() { Hour = 0, Minutes = 4.5f, Seconds = 0f };

    [SerializeField] public ScenarioSettings scenarioSettings;

    private void OnValidate()
    {
        //if (TotalDays < 31) TotalDays = 31;
    }

    public float GetTotalGameDurationInSeconds(int totalDays)
    {
        float calculation = 0f;
        for (int i = 0; i < totalDays; i++)
        {
            calculation += DayCycleDuration.DurationInSeconds;
        }
        return calculation;
    }

    public float GetTotalGameDurationInSeconds()
    {
        float calculation = 0f;
        for (int i = 0; i < TotalDays; i++)
        {
            calculation += DayCycleDuration.DurationInSeconds;
        }
        return calculation;
    }
}

[System.Serializable]
public struct TimeDatas
{
    public float Hour;
    public float Minutes;
    public float Seconds;

    //temps en seconde de une journée dans le jeu
    public float DurationInSeconds => Hour * 3600 + Minutes * 60 + Seconds;

    //échelle à laquelle la vitesse d'une journée dans le jeu est multiplié 
    //par rapport à celle de la vrai vie
    public float DurationScale => 86400f / DurationInSeconds; 

    public TimeDatas(float hour, float minutes, float seconds)    
    {
        Hour = hour;
        Minutes = minutes;
        Seconds = seconds;
    }

   
}