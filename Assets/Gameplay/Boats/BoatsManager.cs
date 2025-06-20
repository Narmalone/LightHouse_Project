using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Game.DayNightSystem;

public class BoatsManager : MonoBehaviour
{
    [Header("Boat Settings")]
    public Boat Prefab;
    public List<Boat> CurrentPresentBoats = new();

    [Header("Spawn Settings")]
    public byte MinBoatsSpawn = 2;
    public byte MaxBoatsSpawn = 4;
    [Range(0f, 24f)] public float spawnStartHour = 20f;
    [Range(0f, 24f)] public float spawnEndHour = 1f;
    public float visualSpawnDelay = 1f;

    [Header("Time Settings")]
    public bool despawnOnMorning = true;

    [SerializeField] private List<BoatAnomaly> possibleAnomalies;
    [SerializeField] private float anomalyChance = 0.3f;

    private void MaybeAddAnomaly(Boat boat)
    {
        if (Random.value <= anomalyChance)
        {
            var anomalyType = possibleAnomalies[Random.Range(0, possibleAnomalies.Count)];
            boat.AnomalyController.AddAnomaly(anomalyType);
        }
    }

    private bool hasSpawnedToday = false;
    private List<float> scheduledTimes = new();
    private int nextIndex = 0;

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

        scheduledTimes.Sort((a, b) => CompareTimeOfDay(a, b));
    }

    private float RandomHourInWindow(float start, float end)
    {
        if (end > start)
        {
            return Random.Range(start, end);
        }
        else
        {
            // Crosses midnight: pick in 0→end or start→24
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
        // Order by time considering wrap-around
        if (IsTimeInRange(a, spawnStartHour, spawnEndHour) && IsTimeInRange(b, spawnStartHour, spawnEndHour))
        {
            return ((a < spawnStartHour) ? a + 24f : a).CompareTo((b < spawnStartHour) ? b + 24f : b);
        }
        return a.CompareTo(b);
    }

    private void SpawnBoat()
    {
        Boat boat = Instantiate(Prefab);
        MaybeAddAnomaly(boat);
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
}
