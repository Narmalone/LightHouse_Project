public class CalendarEventBuilder
{
    private readonly CalendarEvent _e = new();

    public static CalendarEventBuilder New(string description)
        => new CalendarEventBuilder().WithDescription(description);

    public CalendarEventBuilder WithDescription(string desc) { _e.Description = desc; return this; }
    public CalendarEventBuilder At(float start) { _e.StartTime = start; _e.EndTime = -1f; return this; }
    public CalendarEventBuilder FromTo(float start, float end) { _e.StartTime = start; _e.EndTime = end; return this; }
    public CalendarEventBuilder StartDayIs(int day) { _e.StartDay = day; return this; }

    // Récurrences
    public CalendarEventBuilder Once(int day) { _e.Recurrence = Recurrence.Once(day); return this; }
    public CalendarEventBuilder Daily() { _e.Recurrence = Recurrence.DailyForever(); return this; }
    public CalendarEventBuilder EveryNDays(int n, int fromDay) { _e.Recurrence = Recurrence.Every(n, fromDay); return this; }
    public CalendarEventBuilder Weekly(WeekdayMask mask) { _e.Recurrence = Recurrence.Weekly(mask); return this; }
    public CalendarEventBuilder MonthlyOn(int monthDay) { _e.Recurrence = Recurrence.MonthlyOn(monthDay); return this; }
    public CalendarEventBuilder YearlyOn(int dayOfYear) { _e.Recurrence = Recurrence.YearlyOn(dayOfYear); return this; }
    public CalendarEventBuilder Until(int lastDay) { _e.Recurrence.UntilDay = lastDay; return this; }
    public CalendarEventBuilder Limit(int count) { _e.Recurrence.CountLimit = count; return this; }

    public CalendarEvent Build() => _e;
}
