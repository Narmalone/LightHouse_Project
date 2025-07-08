using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CalendarEventDatabase", menuName = "Calendar/Calendar Event Database")]
public class CalendarEventDatabase : ScriptableObject
{
    public List<CalendarEvent> events = new List<CalendarEvent>();
    public CalendarEvent[] startingEvents;

    public List<CalendarEvent> GetEventsForDay(byte day)
    {
        return events.FindAll(e => e.Day == day);
    }

    public List<CalendarEvent> GetEventsForDayAndTime(byte day, float time)
    {
        return events.FindAll(e =>
            e.Day == day &&
            (
                (!e.IsTimedEvent && Mathf.Approximately(e.StartTime, time)) ||
                (e.IsTimedEvent && time >= e.StartTime && time <= e.EndTime)
            )
        );
    }

    public void AddEvent(CalendarEvent e)
    {
        events.Add(e);
    }

    public void RemoveEventsForDay(byte day)
    {
        events.RemoveAll(e => e.Day == day);
    }
}
