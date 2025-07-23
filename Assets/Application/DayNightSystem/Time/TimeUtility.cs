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


        /// <summary>
        /// Retourne true si l'heure actuelle a atteint ou dépassé une heure cible (dans une plage cyclique).
        /// </summary>
        public static bool HasReachedHour(float now, float target, float end)
        {
            return IsTimeInRange(now, target, end);
        }

        /// <summary>
        /// Vérifie si l'heure actuelle se situe entre deux bornes horaires (supporte les cycles jour/nuit).
        /// Exemple : IsTimeInRange(2h, 22h, 6h) → true.
        /// </summary>
        public static bool IsTimeInRange(float current, float start, float end)
        {
            if (end > start)
                return current >= start && current < end;
            else
                return current >= start || current < end;
        }

        /// <summary>
        /// Compare deux heures dans une plage donnée.
        /// Utile quand la plage peut traverser minuit.
        /// </summary>
        public static int CompareTimeOfDay(float a, float b, float startHour, float endHour)
        {
            bool inRangeA = IsTimeInRange(a, startHour, endHour);
            bool inRangeB = IsTimeInRange(b, startHour, endHour);

            if (inRangeA && inRangeB)
            {
                float adjustedA = a < startHour ? a + 24f : a;
                float adjustedB = b < startHour ? b + 24f : b;
                return adjustedA.CompareTo(adjustedB);
            }

            return a.CompareTo(b);
        }
    }
}
