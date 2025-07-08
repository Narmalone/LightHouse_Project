using LightHouse.Game.Computer.OS;
using LightHouse.Game.DayNightSystem;
using TMPro;
using UnityEngine;

namespace LightHouse.Game.Computer.Calendar
{
    public class CalendarShortCut : ShortCutController
    {
        [SerializeField] private TextMeshProUGUI _calendarDate;

        protected override void Awake()
        {
            base.Awake();
            TimeHandlerData.OnDayChanged += OnDayUpdated;
        }

        private void Start()
        {
            UpdateCalendarDay(TimeHandlerData.CurrentDay);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TimeHandlerData.OnDayChanged -= OnDayUpdated;
        }

        private void OnDayUpdated(byte obj)
        {
            UpdateCalendarDay(obj);
        }

        public void UpdateCalendarDay(byte day)
        {
            _calendarDate.text = day.ToString("D2");
        }
    }

}
