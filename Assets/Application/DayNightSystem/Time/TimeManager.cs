using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
    public class TimeManager : MonoBehaviour
    {
        [Range(0f, 24f)]
        public float currentTime = 6f; // Heure initiale
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
            currentTime = (currentTime + delta) % 24f;
            NotifyObservers();
        }

        private void NotifyObservers()
        {
            foreach (var observer in observers)
                observer.OnTimeChanged(currentTime);
        }
    }
}
