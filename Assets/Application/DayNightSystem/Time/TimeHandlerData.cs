using System;

namespace LightHouse.Game.DayNightSystem
{
    /// <summary>
    /// Contient les données globales du système jour/nuit accessibles à tout moment.
    /// Permet également d’écouter les événements liés au changement de segment horaire ou de jour.
    /// </summary>
    public static class TimeHandlerData
    {
        #region Public Time State

        /// <summary>
        /// Temps actuel dans la journée (ex : 13.5 = 13h30).
        /// </summary>
        public static float CurrentTime;

        /// <summary>
        /// Jour actuel (démarre à 1).
        /// </summary>
        public static byte CurrentDay;

        /// <summary>
        /// Segment de la journée actuel (matin, midi, soir, nuit, etc.).
        /// </summary>
        public static TimeOfDaySegment TimeOfDay;

        #endregion

        #region Events

        /// <summary>
        /// Événement appelé lorsque le segment de la journée change.
        /// </summary>
        public static Action<TimeOfDaySegment> OnTimeSegmentChanged;

        /// <summary>
        /// Événement appelé lorsque le jour change.
        /// </summary>
        public static Action<byte> OnDayChanged;

        /// <summary>
        /// Événement appelé lorsque la journée atteint sa fin.
        /// </summary>
        public static Action OnTimeReachesEnd;

        #endregion
    }
}
