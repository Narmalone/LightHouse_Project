using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
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
    }
}
