using TMPro;
using UnityEngine;
using LightHouse.Game.Computer.OS;
using LightHouse.Game.DayNightSystem;
using System.Text;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.Calendar
{
    /// <summary>
    /// Application calendrier affichant les événements journaliers
    /// et l'heure actuelle au format 12h (AM/PM).
    /// </summary>
    public class CalendarApp : ComputerApp
    {
        #region UI References

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _dayType;
        [SerializeField] private TextMeshProUGUI _dayTime;
        [SerializeField] private TextMeshProUGUI _summaryDayText;
        [SerializeField] private TextMeshProUGUI _daySummary;
        [SerializeField] private ClockHandRotator _handRotator;
        [SerializeField] private ColorSettings _basicColors;

        #endregion

        #region Calendar Data

        [Header("Calendar Data")]
        [SerializeField] private CalendarEventDatabase _eventDatabase;
        [SerializeField] private Transform _parentDaysToCheck;
        [SerializeField] private CalendarDayElement[] _calendarDays;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            TimeHandlerData.OnDayChanged += OnDayChanged;

            // Abonnement au clic sur chaque jour
            for (int i = 0; i < _calendarDays.Length; i++)
            {
                int index = i; // évite la capture incorrecte dans la closure
                _calendarDays[i].Button.onClick.AddListener(() => OnClickDay(index));
            }
        }

        private void Start()
        {
            byte currentDay = TimeHandlerData.CurrentDay;

            UpdateCurrentDayUI(currentDay); // UI sur le jour en cours
            DistributeStartingEventsToCalendar(); // événements initiaux
            ShowDaySummary((byte)TimeUtility.ToIndexFromDay(currentDay)); // résumé du jour
        }

        private void LateUpdate()
        {
            SetDayTime(TimeHandlerData.CurrentDay, TimeHandlerData.CurrentTime);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TimeHandlerData.OnDayChanged -= OnDayChanged;

            // Désinscription des clics
            foreach (var day in _calendarDays)
                day.Button.onClick.RemoveAllListeners();
        }

        private void OnValidate()
        {
            _calendarDays = _parentDaysToCheck.GetComponentsInChildren<CalendarDayElement>();
        }

        #endregion

        #region Calendar Logic

        private void OnDayChanged(byte currentDay)
        {
            int index = TimeUtility.ToIndexFromDay(currentDay);
            if (index >= _calendarDays.Length) return;

            UpdateDathPastUI(currentDay);
            UpdateCurrentDayUI(currentDay);
            ShowDaySummary((byte)index);
        }

        private void UpdateDathPastUI(byte currentDay)
        {
            if (currentDay < 2) return;

            var target = _calendarDays[currentDay - 2];
            ApplyColorToDay(target, _basicColors.ScrollbarBorder, _basicColors.IconTint, _basicColors.WindowBorder);
        }

        private void UpdateCurrentDayUI(byte currentDay)
        {
            var target = _calendarDays[currentDay - 1];
            ApplyColorToDay(target, _basicColors.ButtonCloseBorder, _basicColors.IconTint, _basicColors.ButtonCloseBorder);
        }

        /// <summary>
        /// Applique des couleurs personnalisées à un jour du calendrier.
        /// </summary>
        private void ApplyColorToDay(CalendarDayElement day, Color normal, Color highlight, Color shadow)
        {
            ColorBlock block = day.Button.colors;
            block.normalColor = normal;
            block.highlightedColor = highlight;
            day.Shadow.effectColor = shadow;
            day.Button.colors = block;
        }

        /// <summary>
        /// Remplit chaque jour du calendrier avec les événements initiaux.
        /// </summary>
        private void DistributeStartingEventsToCalendar()
        {
            if (_eventDatabase == null || _calendarDays == null || _calendarDays.Length == 0)
            {
                Debug.LogWarning("Calendar not properly set up.");
                return;
            }

            foreach (var day in _calendarDays)
                day.Events.Clear();

            foreach (var evt in _eventDatabase.startingEvents)
            {
                if (evt == null) continue;

                for (int i = 0; i < _calendarDays.Length; i++)
                {
                    byte targetDay = (byte)i;
                    if (evt.Matches(targetDay, evt.StartTime))
                        _calendarDays[i].AddEvent(evt);
                }
            }
        }

        /// <summary>
        /// Affiche un résumé textuel des événements d’un jour donné.
        /// </summary>
        public void ShowDaySummary(byte dayIndex)
        {
            if (dayIndex >= _calendarDays.Length) return;

            var events = _calendarDays[dayIndex].Events;

            if (events == null || events.Count == 0)
            {
                _daySummary.text = $"Day {dayIndex + 1:D2}:\n- No events";
                return;
            }

            //Handle Notification
            if (_calendarDays[dayIndex].NotificationImg.isActiveAndEnabled)
                _calendarDays[dayIndex].NotificationImg.gameObject.SetActive(false);

            //Sort by Start Time for UX
            events.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            _summaryDayText.text = $"Day {dayIndex + 1:D2}:";

            //Concat everything in one single string about all events
            StringBuilder sb = new();
            foreach (var e in events)
            {
                string timeStr = e.IsTimedEvent
                    ? $"{TimeUtility.FormatTime12h(e.StartTime)} - {TimeUtility.FormatTime12h(e.EndTime)}"
                    : TimeUtility.FormatTime12h(e.StartTime);

                sb.AppendLine($"- {timeStr}: {e.Description}");
            }

            _daySummary.text = sb.ToString();
        }

        /// <summary>
        /// Clic sur un jour : affiche le résumé correspondant.
        /// </summary>
        public void OnClickDay(int dayIndex)
        {
            ShowDaySummary((byte)dayIndex);
        }

        /// <summary>
        /// Met à jour l'affichage de l'heure dans l'UI.
        /// </summary>
        public void SetDayTime(byte day, float time)
        {
            _dayTime.text = $"Day {day:D2} - {TimeUtility.FormatTime12h(time)}";
        }

        #endregion

        #region App Lifecycle Overrides

        public override void OnClose()
        {
            if (OpenMode == AppOpenMode.ReactivateIfExists)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }

        public override void OnMinimize() { }
        public override void OnOpen() { }

        #endregion
    }
}
