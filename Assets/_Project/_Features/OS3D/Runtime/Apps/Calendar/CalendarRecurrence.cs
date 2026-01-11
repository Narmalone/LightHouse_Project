using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Computer.Calendar
{
    public enum RecurrenceKind
    {
        None,               // une seule fois
        Daily,              // tous les jours
        EveryNDays,         // tous les N jours (ancré sur un jour de départ)
        WeeklyOn,           // certains jours de la semaine (ex: Lundi+Jeudi)
        MonthlyOnDay,       // chaque mois, le jour X (ex: le 15)
        YearlyOnDayOfYear   // chaque année, le jour Y de l’année (0..364)
    }

    [System.Flags]
    public enum WeekdayMask : byte
    {
        None = 0,
        Mon = 1 << 0,
        Tue = 1 << 1,
        Wed = 1 << 2,
        Thu = 1 << 3,
        Fri = 1 << 4,
        Sat = 1 << 5,
        Sun = 1 << 6,
        All = Mon | Tue | Wed | Thu | Fri | Sat | Sun
    }

    /// <summary>
    /// Spécifie la règle de récurrence. Tous les champs ne sont pas utilisés pour chaque Kind.
    /// </summary>
    [System.Serializable]
    public struct Recurrence
    {
        public RecurrenceKind Kind;

        [Tooltip("Pour EveryNDays: intervalle N. Pour WeeklyOn: ignoré. Pour MonthlyOnDay/YearlyOnDayOfYear: ignoré.")]
        public int Interval;

        [Tooltip("Jour d’ancrage pour EveryNDays (ex: 10 => commence au jour 10).")]
        public int AnchorDay;

        [Tooltip("Pour WeeklyOn: combinaison de jours (flags).")]
        public WeekdayMask Weekdays;

        [Tooltip("Pour MonthlyOnDay: jour du mois (1..30/31 selon ton calendrier).")]
        public int MonthDay;

        [Tooltip("Pour YearlyOnDayOfYear: index 0..364.")]
        public int DayOfYear;

        [Tooltip("Stop après ce jour inclus (optionnel). -1 = infini.")]
        public int UntilDay;

        [Tooltip("Limite de nombre d’occurrences (optionnel). 0/neg = infini.")]
        public int CountLimit;

        public bool Matches(int currentDay)
        {
            if (UntilDay >= 0 && currentDay > UntilDay) return false;

            switch (Kind)
            {
                case RecurrenceKind.None:
                    return currentDay == AnchorDay;

                case RecurrenceKind.Daily:
                    return true;

                case RecurrenceKind.EveryNDays:
                    if (currentDay < AnchorDay || Interval <= 0) return false;
                    return ((currentDay - AnchorDay) % Interval) == 0;

                case RecurrenceKind.WeeklyOn:
                    // suppose day 0 = Lundi (adapte si besoin)
                    var weekday = (WeekdayMask)(1 << (currentDay % 7));
                    return (Weekdays & weekday) != 0;

                case RecurrenceKind.MonthlyOnDay:
                    // si ton calendrier a 30 jours par mois : jourGlobal % 30 → 0..29
                    int dayInMonth = (currentDay % 30) + 1; // 1..30
                    return dayInMonth == MonthDay;

                case RecurrenceKind.YearlyOnDayOfYear:
                    // si ton année = 360/365 selon ton jeu; ici 365 par ex.
                    int diy = currentDay % 365;
                    return diy == DayOfYear;
            }
            return false;
        }

        /// <summary>
        /// Génère les jours d’occurrence dans [startDay, endDay] (inclus).
        /// </summary>
        public IEnumerable<int> Enumerate(int startDay, int endDay)
        {
            if (endDay < startDay) yield break;

            int emitted = 0;

            if (Kind == RecurrenceKind.None)
            {
                if (AnchorDay >= startDay && AnchorDay <= endDay)
                    yield return AnchorDay;
                yield break;
            }

            // borne supérieure si UntilDay est défini
            if (UntilDay >= 0) endDay = Mathf.Min(endDay, UntilDay);

            switch (Kind)
            {
                case RecurrenceKind.Daily:
                    {
                        for (int d = startDay; d <= endDay; d++)
                        {
                            yield return d;
                            if (CountLimit > 0 && ++emitted >= CountLimit) yield break;
                        }
                        break;
                    }

                case RecurrenceKind.EveryNDays:
                    {
                        if (Interval <= 0) yield break;
                        // premier >= startDay qui respecte (d-Anchor)%Interval==0
                        int d0 = Mathf.Max(startDay, AnchorDay);
                        int offset = (d0 - AnchorDay) % Interval;
                        if (offset != 0) d0 += (Interval - offset);

                        for (int d = d0; d <= endDay; d += Interval)
                        {
                            yield return d;
                            if (CountLimit > 0 && ++emitted >= CountLimit) yield break;
                        }
                        break;
                    }

                case RecurrenceKind.WeeklyOn:
                    {
                        for (int d = startDay; d <= endDay; d++)
                        {
                            var weekday = (WeekdayMask)(1 << (d % 7));
                            if ((Weekdays & weekday) != 0)
                            {
                                yield return d;
                                if (CountLimit > 0 && ++emitted >= CountLimit) yield break;
                            }
                        }
                        break;
                    }

                case RecurrenceKind.MonthlyOnDay:
                    {
                        for (int d = startDay; d <= endDay; d++)
                        {
                            int dayInMonth = (d % 30) + 1;
                            if (dayInMonth == MonthDay)
                            {
                                yield return d;
                                if (CountLimit > 0 && ++emitted >= CountLimit) yield break;
                            }
                        }
                        break;
                    }

                case RecurrenceKind.YearlyOnDayOfYear:
                    {
                        for (int d = startDay; d <= endDay; d++)
                        {
                            int diy = d % 365;
                            if (diy == DayOfYear)
                            {
                                yield return d;
                                if (CountLimit > 0 && ++emitted >= CountLimit) yield break;
                            }
                        }
                        break;
                    }
            }
        }

        // Helpers statiques pour une déclaration super simple :
        public static Recurrence Once(int day) => new Recurrence { Kind = RecurrenceKind.None, AnchorDay = day, UntilDay = -1 };
        public static Recurrence DailyForever() => new Recurrence { Kind = RecurrenceKind.Daily, UntilDay = -1 };
        public static Recurrence Every(int nDays, int startDay) => new Recurrence { Kind = RecurrenceKind.EveryNDays, Interval = nDays, AnchorDay = startDay, UntilDay = -1 };
        public static Recurrence Weekly(WeekdayMask days) => new Recurrence { Kind = RecurrenceKind.WeeklyOn, Weekdays = days, UntilDay = -1 };
        public static Recurrence MonthlyOn(int monthDay) => new Recurrence { Kind = RecurrenceKind.MonthlyOnDay, MonthDay = monthDay, UntilDay = -1 };
        public static Recurrence YearlyOn(int dayOfYear) => new Recurrence { Kind = RecurrenceKind.YearlyOnDayOfYear, DayOfYear = dayOfYear, UntilDay = -1 };
    }

}
