using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CalendarDayElement : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button _dayElementButton;
    [SerializeField] private Shadow _shadow;
    [SerializeField] private Image _notificationImg;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private List<CalendarEvent> _events = new List<CalendarEvent>();

    public Button Button => _dayElementButton;
    public Shadow Shadow => _shadow;
    public TextMeshProUGUI Text => _dayText;
    public List<CalendarEvent> Events => _events;
    public Image NotificationImg => _notificationImg;

    public void AddEvent(CalendarEvent evt)
    {
        _events.Add(evt);
        if (!_notificationImg.isActiveAndEnabled) _notificationImg.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_notificationImg.isActiveAndEnabled) _notificationImg.gameObject.SetActive(false);
    }
}
