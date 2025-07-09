using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Computer.Calendar
{
    /// <summary>
    /// Base de données des événements du calendrier.
    /// Contient les événements ajoutés dynamiquement et les événements de départ configurés en amont.
    /// </summary>
    [CreateAssetMenu(fileName = "CalendarEventDatabase", menuName = "Calendar/Calendar Event Database")]
    public class CalendarEventDatabase : ScriptableObject
    {
        [Header("Events Data")]
        public List<CalendarEvent> events = new();            // Événements ajoutés dynamiquement
        public CalendarEvent[] startingEvents;                 // Événements définis à l’avance (ex. : via l’inspecteur)

        #region Public API

        /// <summary>
        /// Retourne tous les événements correspondant à un jour spécifique.
        /// </summary>
        public List<CalendarEvent> GetEventsForDay(byte day)
        {
            return events.FindAll(e => e.Day == day);
        }

        /// <summary>
        /// Retourne tous les événements actifs à une heure précise d’un jour donné.
        /// </summary>
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

        /// <summary>
        /// Ajoute un événement dynamiquement à la base.
        /// </summary>
        public void AddEvent(CalendarEvent e)
        {
            events.Add(e);
        }

        /// <summary>
        /// Supprime tous les événements liés à un jour spécifique.
        /// </summary>
        public void RemoveEventsForDay(byte day)
        {
            events.RemoveAll(e => e.Day == day);
        }

        #endregion
    }
}
