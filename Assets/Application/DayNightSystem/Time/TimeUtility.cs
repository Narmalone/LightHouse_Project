using System.Text.RegularExpressions;
using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
    /// <summary>
    /// Time Utility contains utilities functions that helps to manage about Day Night & Time System
    /// You can check the main system about: <see cref="TimeManager"/> <seealso cref="TimeHandlerData"/> classes
    /// </summary>
    public static class TimeUtility
    {
        public static int ToIndexFromDay(byte day) => Mathf.Max(day - 1, 0);
        public static byte ToDayFromIndex(int index) => (byte)(index + 1);

        /// <summary>
        /// Formate une heure décimale en format 12h (AM/PM).
        /// </summary>
        public static string FormatTime12h(float time)
        {
            int hour = Mathf.FloorToInt(time);
            int minute = Mathf.RoundToInt((time - hour) * 60);

            string period = hour < 12 ? "AM" : "PM";
            int displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;

            return $"{displayHour:D2}:{minute:D2} {period}";
        }

        /// <summary>
        /// Retourne une chaîne formatée "day XX" à partir d'un numéro de jour.
        /// </summary>
        public static string FormatDay(byte day)
        {
            return $"day {day:D2}";
        }

    }
}
