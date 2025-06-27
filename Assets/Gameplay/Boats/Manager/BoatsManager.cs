using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Game.DayNightSystem;
using LightHouse.Weather;

public class BoatsManager : MonoBehaviour
{
    #region === Inspector Fields ===

    [Header("Boat Settings")]
    public Boat Prefab;
    public List<Boat> CurrentPresentBoats = new();
    public AnomalyConfiguration[] AnomalyConfigurations;
    public BoatAnomalyDefinition[] allAnomalyDefinitions;

    [Header("Spawn Settings")]
    public byte MinBoatsSpawn = 2;
    public byte MaxBoatsSpawn = 4;
    [Range(0f, 24f)] public float spawnStartHour = 20f;
    [Range(0f, 24f)] public float spawnEndHour = 1f;
    public float visualSpawnDelay = 1f;

    [Header("Time Settings")]
    public bool despawnOnMorning = true;

    [Header("Anomalies")]
    [SerializeField] private float anomalyChance = 0.3f;

    #endregion

    #region === Private Fields ===

    private bool hasSpawnedToday = false;
    private List<float> scheduledTimes = new();
    private int nextIndex = 0;

    #endregion

    #region === Unity Events ===

    private void OnEnable()
    {
        TimeManager.OnTimeSegmentChanged += HandleTimeSegmentChange;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeSegmentChanged -= HandleTimeSegmentChange;
    }

    private void Start()
    {
        if (IsTimeInRange(TimeHandlerData.CurrentTime, spawnStartHour, spawnEndHour))
        {
            ScheduleSpawns();
            hasSpawnedToday = true;
        }
    }

    private void Update()
    {
        float now = TimeHandlerData.CurrentTime;

        if (!hasSpawnedToday && IsTimeInRange(now, spawnStartHour, spawnEndHour))
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

        int count = Random.Range(MinBoatsSpawn, MaxBoatsSpawn + 1);

        for (int i = 0; i < count; i++)
        {
            float randomHour = RandomHourInWindow(spawnStartHour, spawnEndHour);
            scheduledTimes.Add(randomHour);
        }

        scheduledTimes.Sort(CompareTimeOfDay);
    }

    private void SpawnBoat()
    {
        Boat boat = Instantiate(Prefab);
        TryAddAnomaly(boat);
        CurrentPresentBoats.Add(boat);

        if (visualSpawnDelay > 0f)
            StartCoroutine(VisualDelay());
    }

    private IEnumerator VisualDelay()
    {
        yield return new WaitForSeconds(visualSpawnDelay);
    }

    private void DespawnAllBoats()
    {
        foreach (var boat in CurrentPresentBoats)
        {
            if (boat != null)
                Destroy(boat.gameObject);
        }

        CurrentPresentBoats.Clear();
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

            if (despawnOnMorning)
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
        return IsTimeInRange(now, target, spawnEndHour);
    }

    private bool IsTimeInRange(float current, float start, float end)
    {
        if (end > start)
            return current >= start && current < end;
        else
            return current >= start || current < end;
    }

    private int CompareTimeOfDay(float a, float b)
    {
        if (IsTimeInRange(a, spawnStartHour, spawnEndHour) &&
            IsTimeInRange(b, spawnStartHour, spawnEndHour))
        {
            return ((a < spawnStartHour) ? a + 24f : a).CompareTo((b < spawnStartHour) ? b + 24f : b);
        }

        return a.CompareTo(b);
    }

    #endregion

    #region === Anomaly Management ===

    /// <summary>
    /// Possibly attaches a random anomaly to a boat based on weather configuration.
    /// </summary>
    private void TryAddAnomaly(Boat boat)
    {
        if (Random.value > anomalyChance) return;

        foreach (var config in AnomalyConfigurations)
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
        foreach (var def in allAnomalyDefinitions)
            if (def.Type == type)
                return def;

        return null;
    }

    #endregion
}
