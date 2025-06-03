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

    public class TimeManager : MonoBehaviour
    {
        [Range(0f, 24f)]
        public float currentTime = 6f; // Heure initiale
        public int currentDay = 0;
        public TimeOfDaySegment CurrentSegment { get; private set; }
        public event Action<TimeOfDaySegment> OnTimeSegmentChanged;

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

        private void Update()
        {
            float delta = (24f / (TimeConfig.dayLengthInMinits * 60f)) * Time.deltaTime;
            currentTime += delta;

            if (currentTime >= 24f)
            {
                currentTime %= 24f;
                currentDay++;
            }

            UpdateTimeSegment();
            NotifyObservers();
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
                OnTimeSegmentChanged?.Invoke(newSegment);
            }
        }


        private void NotifyObservers()
        {
            foreach (var observer in observers)
                observer.OnTimeChanged(currentTime);
        }
    }
}
