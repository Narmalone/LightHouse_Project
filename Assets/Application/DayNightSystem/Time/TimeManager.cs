using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
    public enum TimeOfDaySegment
    {
        Night,
        Morning,
        Midday,
        Evening
    }

    public enum TimeOfDaySecondariesSegments
    {
        StartMorningTransition,
        EndMorningTransition,
        StartDayTransition,
        EndDayTransition,
        StartEveningTransition,
        EndEveningTransition,
        StartNightTransition,
        EndNightTransition
    }

    public class TimeManager : MonoBehaviour
    {
        [Range(0f, 24f)]
        public float currentTime = 6f; // Heure initiale
        public byte currentDay = 1;
        [field: SerializeField] public TimeOfDaySegment CurrentSegment { get; private set; }
        private TimeOfDaySegment lastSegment;

        public TimeConfiguration TimeConfig;

        private List<ITimeCycleObserver> observers = new List<ITimeCycleObserver>();

        public void RegisterObserver(ITimeCycleObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void UnregisterObserver(ITimeCycleObserver observer)
        {
            if (observers.Contains(observer))
                observers.Remove(observer);
        }

        private void Awake()
        {
            //currentDay = 1;
            TimeHandlerData.CurrentDay = currentDay;
            TimeHandlerData.CurrentTime = currentTime;
        }

        private void Update()
        {
            float delta = (24f / (TimeConfig.dayLengthInMinits * 60f)) * Time.deltaTime;
            currentTime += delta;
            currentTime = Mathf.Clamp(currentTime, 0f, 24f);

            if (currentTime >= 24f)
            {
                currentTime %= 24f;
                currentDay++;
                TimeHandlerData.CurrentDay = currentDay;
                TimeHandlerData.OnDayChanged?.Invoke(currentDay);

                if (currentDay >= TimeConfig.TotalDays)
                {
                    TimeHandlerData.OnTimeReachesEnd?.Invoke();
                }
            }

            TimeHandlerData.CurrentTime = currentTime;
            UpdateTimeSegment();
            NotifyObservers();
            TimeHandlerData.OnTimeChanged?.Invoke(currentTime);
        }

        private void UpdateTimeSegment()
        {
            TimeOfDaySegment newSegment;

            if (currentTime >= 6f && currentTime < 12f)
                newSegment = TimeOfDaySegment.Morning;
            else if (currentTime >= 12f && currentTime < 18f)
                newSegment = TimeOfDaySegment.Midday;
            else if (currentTime >= 18f && currentTime < 24f)
                newSegment = TimeOfDaySegment.Evening;
            else
                newSegment = TimeOfDaySegment.Night;

            if (newSegment != lastSegment)
            {
                lastSegment = newSegment;
                CurrentSegment = newSegment;
                TimeHandlerData.TimeOfDay = newSegment;
                TimeHandlerData.OnTimeSegmentChanged?.Invoke(newSegment);
            }
        }


        private void NotifyObservers()
        {
            foreach (var observer in observers)
                observer.OnTimeChanged(currentTime);
        }
    }
}
