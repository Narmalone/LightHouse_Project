using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.TimeOfDay.TimeCore
{
    public enum TimeOfDaySegment
    {
        Night,
        Morning,
        Midday,
        Evening
    }

    public class TimeManager : MonoBehaviour
    {
        [Range(0f, 24f)]
        public float CurrentTime = 6f; // Heure initiale
        public byte CurentDay = 1;
        public float TimeSpeed = 1.0f;

        [field: SerializeField] public TimeOfDaySegment CurrentSegment { get; private set; }
        private TimeOfDaySegment _lastSegment;

        public TimeConfiguration TimeConfig;

        private void Awake()
        {
            //currentDay = 1;
            TimeHandlerData.CurrentDay = CurentDay;
            TimeHandlerData.CurrentTime = CurrentTime;
        }

        private void Update()
        {
            float deltaHours = (Time.deltaTime / TimeConfig.RealSecondsPerGameHour) * TimeSpeed;
            CurrentTime += deltaHours;

            // NOTE: on ne clamp plus AVANT de tester le dépassement de minuit.
            // Bug précédent : Mathf.Clamp(CurrentTime, 0f, 24f) était appliqué avant le "if (>= 24f)",
            // donc CurrentTime était tronqué pile à 24.0 puis remis à 0 via %=24f, ce qui faisait
            // perdre la fraction d'heure qui dépassait minuit à chaque frame de rollover (petit
            // micro-saut/gel du cycle jour-nuit une fois par jour de jeu). On gère maintenant le
            // dépassement en préservant le reste, et de façon robuste même si TimeSpeed/deltaTime
            // fait sauter plusieurs jours d'un coup (rare, mais ne casse plus rien).
            if (CurrentTime >= 24f)
            {
                int daysToAdd = Mathf.FloorToInt(CurrentTime / 24f);
                CurrentTime -= daysToAdd * 24f;

                for (int i = 0; i < daysToAdd; i++)
                {
                    CurentDay++;
                    TimeHandlerData.CurrentDay = CurentDay;
                    TimeHandlerData.OnDayChanged?.Invoke(CurentDay);

                    if (CurentDay >= TimeConfig.TotalDays)
                    {
                        TimeHandlerData.OnTimeReachesEnd?.Invoke();
                        break;
                    }
                }
            }
            else if (CurrentTime < 0f)
            {
                // Garde-fou : ne devrait pas arriver (TimeSpeed négatif non prévu), mais on
                // évite de faire planter la logique de segments si jamais.
                CurrentTime = 0f;
            }

            TimeHandlerData.CurrentTime = CurrentTime;
            UpdateTimeSegment();
            TimeHandlerData.OnTimeChanged?.Invoke(CurrentTime);
        }

        private void UpdateTimeSegment()
        {
            TimeOfDaySegment newSegment;

            if (CurrentTime >= 6f && CurrentTime < 12f)
                newSegment = TimeOfDaySegment.Morning;
            else if (CurrentTime >= 12f && CurrentTime < 18f)
                newSegment = TimeOfDaySegment.Midday;
            else if (CurrentTime >= 18f && CurrentTime < 24f)
                newSegment = TimeOfDaySegment.Evening;
            else
                newSegment = TimeOfDaySegment.Night;

            if (newSegment != _lastSegment)
            {
                _lastSegment = newSegment;
                CurrentSegment = newSegment;
                TimeHandlerData.TimeOfDay = newSegment;
                TimeHandlerData.OnTimeSegmentChanged?.Invoke(newSegment);
            }
        }
    }
}
