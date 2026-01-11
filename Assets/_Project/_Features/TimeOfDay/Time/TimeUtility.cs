using UnityEngine;

namespace LightHouse.Features.TimeOfDay.TimeCore
{
    /// <summary>
    /// Time Utility contains utilities functions that helps to manage about Day Night & Time System
    /// You can check the main system about: <see cref="TimeManager"/> <seealso cref="TimeHandlerData"/> classes
    /// </summary>
    public static class TimeUtility
    {
        public static int ToIndexFromDay(byte day) => Mathf.Max(day - 1, 0);
        public static byte ToDayFromIndex(int index) => (byte)(index + 1);

        public static float Normalize24h(float h)
        {
            h %= 24f;
            if (h < 0f) h += 24f;
            return h;
        }

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
        /// Formate la date
        /// </summary>
        public static string FormatDate(int day, float time)
        {
            int hour = Mathf.FloorToInt(time);
            int minute = Mathf.RoundToInt((time - hour) * 60);

            string period = hour < 12 ? "AM" : "PM";
            int displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;

            return $"Day {day:D2} - {displayHour:D2}:{minute:D2} {period}";
        }

        /// <summary>
        /// Formate la date actuelle en jeu
        /// </summary>
        /// <returns></returns>
        public static string FormatCurrentDate()
        {
            float currentTime = TimeHandlerData.CurrentTime;
            int hour = Mathf.FloorToInt(currentTime);
            int minute = Mathf.RoundToInt((currentTime - hour) * 60);

            string period = hour < 12 ? "AM" : "PM";
            int displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;

            return $"Day {TimeHandlerData.CurrentDay:D2} - {displayHour:D2}:{minute:D2} {period}";
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
        /// Retourne true si l'heure actuelle a atteint ou dépassé une heure cible (dans une plage cyclique).
        /// </summary>
        public static bool HasReachedDate(int day, float time)
        {
            return TimeHandlerData.CurrentDay == day && TimeHandlerData.CurrentTime > time;
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

        /// <summary>
        /// Calcule le (jour, heure) in-game atteints dans <paramref name="hoursAhead"/> heures
        /// à partir de la date/heure ACTUELLES (TimeHandlerData.CurrentDay/CurrentTime).
        /// Ex: si on est Day 3 - 21:00 et hoursAhead=12 → Day 4 - 09:00.
        /// </summary>
        public static void GetDateAfterHours(float hoursAhead, out byte day, out float time)
        {
            GetDateAfterHours(TimeHandlerData.CurrentDay, TimeHandlerData.CurrentTime, hoursAhead, out day, out time);
        }

        /// <summary>
        /// Calcule le (jour, heure) in-game atteints dans <paramref name="hoursAhead"/> heures
        /// en partant de <paramref name="startDay"/> / <paramref name="startHour"/>.
        /// Ajoute 1 jour par tranche de 24h et reporte le reste en heure.
        /// </summary>
        public static void GetDateAfterHours(byte startDay, float startHour, float hoursAhead, out byte day, out float time)
        {
            // Sécurités de base
            float hAhead = Mathf.Max(0f, hoursAhead);

            // Somme des heures à partir de l'heure de départ
            float totalHours = startHour + hAhead;

            // Nombre entier de jours à ajouter + heure restante [0..24)
            int daysToAdd = Mathf.FloorToInt(totalHours / 24f);
            time = Mathf.Repeat(totalHours, 24f);

            // Nouveau jour (on ne touche pas à l'origine : 0, 1, etc.)
            day = (byte)(startDay + daysToAdd);
        }
    }
}
