using UnityEngine;

public enum EventRecurrence
{
    None,     // Se produit une seule fois à un jour donné
    Daily,    // Se répète tous les jours à la même heure
    Weekly,   // Se répète tous les 7 jours à partir de Day
    Monthly,  // Se répète tous les X jours (ex: tous les 30 jours)
    Yearly    // Se répète tous les 365 jours
}

[System.Serializable]
public class CalendarEvent
{
    public byte Day; // Jour d'origine (utile pour "None" ou comme point de départ)

    [Range(0f, 24f)] public float StartTime = 0f;
    [Range(0f, 24f)] public float EndTime = -1f;  // -1 = événement ponctuel

    public EventRecurrence Recurrence = EventRecurrence.None;

    [TextArea] public string Description;

    public bool IsTimedEvent => EndTime >= 0f;

    public bool Matches(byte currentDay, float currentTime)
    {
        // Vérifie la récursivité jour
        bool matchesDay = Recurrence switch
        {
            EventRecurrence.None => currentDay == Day,
            EventRecurrence.Daily => true,
            EventRecurrence.Weekly => (currentDay - Day) % 7 == 0 && (currentDay >= Day),
            EventRecurrence.Monthly => (currentDay - Day) % 30 == 0 && (currentDay >= Day),
            EventRecurrence.Yearly => (currentDay - Day) % 365 == 0 && (currentDay >= Day),
            _ => false
        };

        if (!matchesDay)
            return false;

        // Vérifie l'heure actuelle
        if (IsTimedEvent)
            return currentTime >= StartTime && currentTime <= EndTime;
        else
            return Mathf.Approximately(currentTime, StartTime);
    }
}
