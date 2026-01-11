using LightHouse.Features.Computer.Calendar.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.Calendar
{
    /// <summary>
    /// Représente un jour dans le calendrier, incluant son affichage, ses événements et la notification visuelle.
    /// </summary>
    public class CalendarDayElement : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Button _dayElementButton;
        [SerializeField] private Shadow _shadow;
        [SerializeField] private Image _notificationImg;
        [SerializeField] private TextMeshProUGUI _dayText;

        [Header("Events Data")]
        [SerializeField] private List<CalendarEvent> _events = new();

        #region Properties

        public Button Button => _dayElementButton;
        public Shadow Shadow => _shadow;
        public TextMeshProUGUI Text => _dayText;
        public List<CalendarEvent> Events => _events;
        public Image NotificationImg => _notificationImg;

        #endregion

        #region Public Methods

        /// <summary>
        /// Ajoute un événement à ce jour et affiche une notification visuelle si elle n'est pas déjà active.
        /// </summary>
        public void AddEvent(CalendarEvent evt, bool enableNotification = true)
        {
            _events.Add(evt);
            if (enableNotification && _notificationImg)
                _notificationImg.gameObject.SetActive(true);
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Gère le clic sur le jour du calendrier. Désactive la notification visuelle si elle est active.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_notificationImg.isActiveAndEnabled)
                _notificationImg.gameObject.SetActive(false);
        }

        #endregion
    }
}
