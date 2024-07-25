using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameSettings gameSettings;
    public TextMeshProUGUI dayTxt;
    public TextMeshProUGUI times;

    public static event Action OnDayCycleEnd;
    public static event Action OnNightCycleEnd;

    [SerializeField] private int currentDay = 0;
    private float cycleTimeLeft;
    private bool isDayTime;
    [SerializeField, ConsoleVariable("TimeSpeed"), ConsoleCategory("Gameplay")] private float timeSpeedMultiplier = 1.0f; // Multiplicateur de vitesse initial à 1
    [SerializeField, ConsoleVariable] private float salmamm = 1.0f; // Multiplicateur de vitesse initial à 1

    private void Start()
    {
        StartTimeCycle();
    }

    public void SetTimeSpeedMultiplier(float newSpeedMultiplier)
    {
        timeSpeedMultiplier = newSpeedMultiplier;
    }

    public void StartTimeCycle()
    {
        // Calcul de la durée totale d'un cycle jour-nuit en secondes
        float dayCycleDuration = ConvertToSeconds(gameSettings.DayCycleDuration);
        float nightCycleDuration = ConvertToSeconds(gameSettings.NightCycleDuration);

        // Initialisation du temps restant pour le cycle actuel (commence par le jour)
        cycleTimeLeft = dayCycleDuration;
        isDayTime = true;

        StartCoroutine(TimeCycleRoutine(dayCycleDuration, nightCycleDuration));
    }

    private IEnumerator TimeCycleRoutine(float dayCycleDuration, float nightCycleDuration)
    {
        while (currentDay <= gameSettings.TotalDays)
        {
            while (cycleTimeLeft > 0)
            {
                cycleTimeLeft -= Time.deltaTime * timeSpeedMultiplier;
                UpdateTimeDisplay();
                yield return null;
            }

            if (isDayTime)
            {
                // Fin du cycle de jour, on passe à la nuit
                cycleTimeLeft = nightCycleDuration;
                isDayTime = false;
                OnDayCycleEnd?.Invoke();
            }
            else
            {
                // Fin du cycle de nuit, on passe au jour suivant
                cycleTimeLeft = dayCycleDuration;
                isDayTime = true;
                currentDay++;
                OnNightCycleEnd?.Invoke();
                UpdateDayDisplay();
            }
        }
    }

    private void UpdateTimeDisplay()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(cycleTimeLeft);
        times.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    private void UpdateDayDisplay()
    {
        dayTxt.text = "Day: " + currentDay;
    }

    private float ConvertToSeconds(TimeDatas timeData)
    {
        return (timeData.Hour * 3600) + (timeData.Minutes * 60) + timeData.Seconds;
    }
}