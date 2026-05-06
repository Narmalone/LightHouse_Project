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
            CurrentTime = Mathf.Clamp(CurrentTime, 0f, 24f);

            if (CurrentTime >= 24f)
            {
                CurrentTime %= 24f;
                CurentDay++;
                TimeHandlerData.CurrentDay = CurentDay;
                TimeHandlerData.OnDayChanged?.Invoke(CurentDay);

                if (CurentDay >= TimeConfig.TotalDays)
                {
                    TimeHandlerData.OnTimeReachesEnd?.Invoke();
                }
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
