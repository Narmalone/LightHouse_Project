using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Core.Settings
{
    public class OptionEnumPassive : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _choiceText;
        public Button LeftButton;
        public Button RightButton;

        public event Action OnPrevClicked;
        public event Action OnNextClicked;

        private List<string> _choices = new List<string>();
        private int _shownIndex = 0;

        private void Awake()
        {
            if (LeftButton) LeftButton.onClick.AddListener(() => OnPrevClicked?.Invoke());
            if (RightButton) RightButton.onClick.AddListener(() => OnNextClicked?.Invoke());
        }

        public void SetChoices(List<string> choices)
        {
            _choices = choices ?? new List<string>();
            _shownIndex = Mathf.Clamp(_shownIndex, 0, Mathf.Max(0, _choices.Count - 1));
            Refresh();
        }

        public void ShowIndex(int index) // affichage SANS notifier
        {
            _shownIndex = Mathf.Clamp(index, 0, Mathf.Max(0, _choices.Count - 1));
            Refresh();
        }

        public int ShownIndex => _shownIndex; // uniquement pour debug si besoin
        public int Count => _choices?.Count ?? 0;

        private void Refresh()
        {
            if (_choiceText == null) return;
            if (_choices == null || _choices.Count == 0)
            {
                _choiceText.text = "";
                return;
            }
            _choiceText.text = _choices[_shownIndex];
        }

        private void OnDestroy()
        {
            if (LeftButton) LeftButton.onClick.RemoveAllListeners();
            if (RightButton) RightButton.onClick.RemoveAllListeners();
        }
    }
}