using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Game.DayNightSystem;
using LightHouse.Weather;

public class BoatsManager : MonoBehaviour
{
    #region === Inspector Fields ===

    [Header("Boat Settings")]
    [SerializeField] private Boat _prefab;
    private List<Boat> _controllers = new();
    [SerializeField] private AnomalyConfiguration[] _anomalyConfigsByWeather;
    [SerializeField] private BoatAnomalyDefinition[] _anomalyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private NightWatchConfiguration _nightWatchConfig;
    [SerializeField] private byte _minBoatsSpawnDuringNight = 2;
    [SerializeField] private byte _maxBoatsSpawnDuringNight = 4;

    [Header("Time Settings")]
    [SerializeField] private bool _despawnAllBoatsOnMorning = false;

    [Header("Anomalies")]
    [SerializeField] private float _anomalyChances = 0.3f;

    #endregion

    #region === Private Fields ===

    private bool hasSpawnedToday = false;
    private List<float> scheduledTimes = new();
    private int nextIndex = 0;

    #endregion

    #region === Unity Events ===

    private void OnEnable()
    {
        TimeHandlerData.OnTimeSegmentChanged += HandleTimeSegmentChange;
    }

    private void OnDisable()
    {
        TimeHandlerData.OnTimeSegmentChanged -= HandleTimeSegmentChange;
    }

    private void Start()
    {
        if (TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime, _nightWatchConfig.BoatsSpawnStartHour, _nightWatchConfig.BoatsSpawnEndHour))
        {
            ScheduleSpawns();
            hasSpawnedToday = true;
        }
    }

    private void Update()
    {
        float now = TimeHandlerData.CurrentTime;

        if (!hasSpawnedToday && TimeUtility.IsTimeInRange(now, _nightWatchConfig.BoatsSpawnStartHour, _nightWatchConfig.BoatsSpawnEndHour))
        {
            ScheduleSpawns();
            hasSpawnedToday = true;
        }

        if (scheduledTimes.Count == 0 || nextIndex >= scheduledTimes.Count)
            return;

        if (HasReachedHour(now, scheduledTimes[nextIndex]))
        {
            SpawnBoat();
            nextIndex++;
        }
    }

    #endregion

    #region === Spawning Logic ===

    private void ScheduleSpawns()
    {
        scheduledTimes.Clear();
        nextIndex = 0;

        int count = Random.Range(_minBoatsSpawnDuringNight, _maxBoatsSpawnDuringNight + 1);

        for (int i = 0; i < count; i++)
        {
            float randomHour = RandomHourInWindow(_nightWatchConfig.BoatsSpawnStartHour, _nightWatchConfig.BoatsSpawnEndHour);
            scheduledTimes.Add(randomHour);
        }

        scheduledTimes.Sort(CompareTimeOfDay);
    }

    private void SpawnBoat()
    {
        Boat boat = Instantiate(_prefab);
        TryAddAnomaly(boat);
        _controllers.Add(boat);
    }

    private void DespawnAllBoats()
    {
        foreach (var boat in _controllers)
        {
            if (boat != null)
                Destroy(boat.gameObject);
        }

        _controllers.Clear();
    }

    #endregion

    #region === Time Helpers ===

    private void HandleTimeSegmentChange(TimeOfDaySegment segment)
    {
        if (segment == TimeOfDaySegment.Morning)
        {
            hasSpawnedToday = false;
            scheduledTimes.Clear();
            nextIndex = 0;

            if (_despawnAllBoatsOnMorning)
                DespawnAllBoats();
        }
    }

    private float RandomHourInWindow(float start, float end)
    {
        if (end > start)
        {
            return Random.Range(start, end);
        }
        else
        {
            float span1 = 24f - start;
            float span2 = end;
            float totalSpan = span1 + span2;
            float pick = Random.Range(0f, totalSpan);

            return (pick < span1) ? (start + pick) : (pick - span1);
        }
    }

    private bool HasReachedHour(float now, float target)
    {
        return TimeUtility.HasReachedHour(now, target, _nightWatchConfig.BoatsSpawnEndHour);
    }

    private int CompareTimeOfDay(float a, float b)
    {
        return TimeUtility.CompareTimeOfDay(a, b, _nightWatchConfig.BoatsSpawnStartHour, _nightWatchConfig.BoatsSpawnEndHour);
    }

    #endregion

    #region === Anomaly Management ===

    /// <summary>
    /// Possibly attaches a random anomaly to a boat based on weather configuration.
    /// </summary>
    private void TryAddAnomaly(Boat boat)
    {
        if (Random.value > _anomalyChances) return;

        foreach (var config in _anomalyConfigsByWeather)
        {
#if UNITY_EDITOR
            //Debug
            if(WeatherHandlerData.CurrentWeather == null)
            {
                var anomalyType = config.PickRandomAnomaly();
                var def = GetAnomalyDefinition(anomalyType.Value);

                
                if (def != null)
                {
                    var re = def.InstantiateAndAttach(boat);
                    boat.AnomalyController.AddAnomaly(re);
                }
                else
                    Debug.LogWarning($"No prefab defined for anomaly: {anomalyType.Value}");

                return;
            }
#endif
            if (config.weatherType == WeatherHandlerData.CurrentWeather.WeatherType)
            {
                var anomalyType = config.PickRandomAnomaly();
                var def = GetAnomalyDefinition(anomalyType.Value);

                if (def != null)
                    def.InstantiateAndAttach(boat);
                else
                    Debug.LogWarning($"No prefab defined for anomaly: {anomalyType.Value}");
            }
        }
    }

    /// <summary>
    /// Finds a matching anomaly prefab definition.
    /// </summary>
    private BoatAnomalyDefinition GetAnomalyDefinition(AnomalyType type)
    {
        foreach (var def in _anomalyPrefabs)
            if (def.Type == type)
                return def;

        return null;
    }

    #endregion
}
