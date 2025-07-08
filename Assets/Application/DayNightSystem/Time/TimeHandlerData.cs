using System;

namespace LightHouse.Game.DayNightSystem
{
    public static class TimeHandlerData
    {
        public static float CurrentTime;
        public static byte CurrentDay;
        public static TimeOfDaySegment TimeOfDay;
        public static Action<TimeOfDaySegment> OnTimeSegmentChanged;
        public static Action<byte> OnDayChanged;
        public static Action OnTimeReachesEnd;
    }

}
