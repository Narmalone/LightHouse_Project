using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Computer.Calendar
{
    [CreateAssetMenu(fileName = "CalendarEventDatabase", menuName = "Calendar/Calendar Event Database")]
    public class CalendarEventDatabase : ScriptableObject
    {
        [Header("Events Data")]
        public List<CalendarEvent> events = new();    // ajoutés dynamiquement au runtime
        public CalendarEvent[] startingEvents;         // configurés dans l’inspector

        public event Action<CalendarEvent> OnEventAdded;

        private static IEnumerable<CalendarEvent> ConcatAll(CalendarEvent[] a, List<CalendarEvent> b)
        {
            if (a != null)
                for (int i = 0; i < a.Length; i++)
                    if (a[i] != null) yield return a[i];
            if (b != null)
                for (int i = 0; i < b.Count; i++)
                    if (b[i] != null) yield return b[i];
        }

        /// <summary>
        /// Génère une table jour -> évènements (toutes occurrences qui tombent dans la plage).
        /// </summary>
        public Dictionary<int, List<CalendarEvent>> GetEventsByDayInRange(int startDay, int endDay)
        {
            var map = new Dictionary<int, List<CalendarEvent>>(endDay - startDay + 1);
            foreach (var e in ConcatAll(startingEvents, events))
            {
                // On ne pré-enumère pas (pour rester simple) : on teste juste Recurrence.Matches(day)
                for (int d = startDay; d <= endDay; d++)
                {
                    if (!e.Recurrence.Matches(d)) continue;
                    if (!map.TryGetValue(d, out var list))
                    {
                        list = new List<CalendarEvent>();
                        map[d] = list;
                    }
                    list.Add(e);
                }
            }
            return map;
        }

        // ------- API simple pour manipuler la DB --------

        public void AddEvent(CalendarEvent e)
        {
            events.Add(e);
            OnEventAdded?.Invoke(e);
        }

        public void ClearRuntimeEvents() => events.Clear();

        // Helpers “Builder-like” si tu veux très simple:
        public CalendarEvent AddOnce(string desc, int day, float at)
        {
            var e = new CalendarEvent { Description = desc, StartTime = at, EndTime = -1, StartDay = day, Recurrence = Recurrence.Once(day) };
            AddEvent(e); return e;
        }

        public CalendarEvent AddEveryNDays(string desc, int n, int fromDay, float at, float end = -1f)
        {
            var e = new CalendarEvent { Description = desc, StartTime = at, EndTime = end, StartDay = fromDay, Recurrence = Recurrence.Every(n, fromDay) };
            AddEvent(e); return e;
        }
    }
}
