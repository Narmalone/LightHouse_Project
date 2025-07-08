using TMPro;
using UnityEngine;
using LightHouse.Game.Computer.OS;
using LightHouse.Game.DayNightSystem;
using System.Text;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.Calendar
{
    /// <summary>
    /// Application calendrier affichant le jour et l'heure actuelle au format 12h (AM/PM).
    /// </summary>
    public class CalendarApp : ComputerApp
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _dayType;
        [SerializeField] private TextMeshProUGUI _dayTime;
        [SerializeField] private TextMeshProUGUI _summaryDayText;
        [SerializeField] private TextMeshProUGUI _daySummary;
        [SerializeField] private ClockHandRotator _handRotator;
        [SerializeField] private ColorSettings _basicColors;

        [Header("Calendar Data")]
        [SerializeField] private Transform _parentDaysToCheck;
        [SerializeField] private CalendarDayElement[] _calendarDays;
        [SerializeField] private CalendarEventDatabase _eventDatabase;
        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            TimeHandlerData.OnDayChanged += OnDayChanged;

            // Enregistrer les listeners une seule fois
            for (int i = 0; i < _calendarDays.Length; i++)
            {
                int index = i; // capture locale pour le delegate
                _calendarDays[i].Button.onClick.AddListener(() => OnClickDay(index));
            }
        }

        private void Start()
        {
            byte currentDay = TimeHandlerData.CurrentDay;
            UpdateCurrentDayUI(currentDay); // ✅ car méthode attend un jour (1-based)
            DistributeStartingEventsToCalendar();
            ShowDaySummary((byte)TimeUtility.ToIndexFromDay(currentDay));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TimeHandlerData.OnDayChanged -= OnDayChanged;

            // Nettoyage des listeners
            foreach (var day in _calendarDays)
                day.Button.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            SetDayTime(TimeHandlerData.CurrentDay, TimeHandlerData.CurrentTime);
        }

        #endregion

        #region Calendar Logic

        private void OnDayChanged(byte currentDay)
        {
            int index = TimeUtility.ToIndexFromDay(currentDay);
            if (index >= _calendarDays.Length) return;

            UpdateDathPastUI((byte)currentDay);
            UpdateCurrentDayUI((byte)currentDay);
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

        private void ApplyColorToDay(CalendarDayElement day, Color normal, Color highlight, Color shadow)
        {
            ColorBlock block = day.Button.colors;
            block.normalColor = normal;
            block.highlightedColor = highlight;
            day.Shadow.effectColor = shadow;
            day.Button.colors = block;
        }

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

        public void ShowDaySummary(byte dayIndex)
        {
            if (dayIndex >= _calendarDays.Length) return;

            var events = _calendarDays[dayIndex].Events;

            if (events == null || events.Count == 0)
            {
                _daySummary.text = $"Day {dayIndex + 1:D2}:\n- No events";
                Debug.Log(_calendarDays[dayIndex].transform.name);
                return;
            }

            events.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            _summaryDayText.text = $"Day {dayIndex + 1:D2}:";

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

        public void OnClickDay(int dayIndex)
        {
            ShowDaySummary((byte)dayIndex);
        }

        public void SetDayTime(byte day, float time)
        {
            _dayTime.text = $"Day {day:D2} - {TimeUtility.FormatTime12h(time)}";
        }

        #endregion

        public override void OnClose() => Destroy(gameObject);
        public override void OnMinimize() { }
        public override void OnOpen() { }
    }
}
