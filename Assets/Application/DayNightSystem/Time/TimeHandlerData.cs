using LightHouse.Game.DayNightSystem;
using System;
using UnityEngine;

public static class TimeHandlerData
{
    public static float CurrentTime;
    public static byte CurrentDay;
    public static TimeOfDaySegment TimeOfDay;
    public static Action<TimeOfDaySegment> OnTimeSegmentChanged;
}
