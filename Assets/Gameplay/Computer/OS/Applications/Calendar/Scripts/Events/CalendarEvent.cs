using UnityEngine;

[System.Serializable]
public class CalendarEvent
{
    [Tooltip("Jour d’origine/concept de départ. Sert aussi d’ancre pour certaines récurrences.")]
    public int StartDay;

    [Range(0f, 24f)] public float StartTime = 0f;
    [Range(-1f, 24f)] public float EndTime = -1f; // -1 = ponctuel

    public Recurrence Recurrence;

    [TextArea] public string Description;

    public bool IsTimedEvent => EndTime >= 0f;

    public bool Matches(int currentDay, float currentTime)
    {
        if (!Recurrence.Matches(currentDay)) return false;

        if (IsTimedEvent)
            return currentTime >= StartTime && currentTime <= EndTime;

        // "ponctuel" = instantané : tolérance au lieu d'égalité stricte
        return Mathf.Abs(currentTime - StartTime) < 0.01f;
    }
}
