using LightHouse.Features.Computer.OS;
using LightHouse.Features.TimeOfDay.TimeCore;
using TMPro;
using UnityEngine;

namespace LightHouse.Features.Computer.Calendar
{
    /// <summary>
    /// Gère le raccourci du calendrier sur le bureau.
    /// Affiche le jour actuel sous forme "DD" et met à jour dynamiquement.
    /// </summary>
    public class CalendarShortCut : ShortCutController
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _calendarDate;

        #region Unity Lifecycle

        /// <summary>
        /// Enregistre le callback de changement de jour.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            TimeHandlerData.OnDayChanged += OnDayUpdated;
        }

        /// <summary>
        /// Initialise l'affichage au démarrage.
        /// </summary>
        private void Start()
        {
            UpdateCalendarDay(TimeHandlerData.CurrentDay);
        }

        /// <summary>
        /// Nettoie les callbacks.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            TimeHandlerData.OnDayChanged -= OnDayUpdated;
        }

        #endregion

        #region Day Display

        /// <summary>
        /// Callback quand le jour change.
        /// </summary>
        private void OnDayUpdated(byte newDay)
        {
            UpdateCalendarDay(newDay);
        }

        /// <summary>
        /// Met à jour le texte du raccourci avec le jour actuel (format "01", "02", etc).
        /// </summary>
        public void UpdateCalendarDay(byte day)
        {
            if (_calendarDate != null)
                _calendarDate.text = day.ToString("D2");
        }

        #endregion
    }
}
