using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Interactive guess bubble.
    /// </summary>
    public class MastermindGuessSlotView :
        MonoBehaviour
    {
        #region Events

        public event Action<int> OnClicked;

        #endregion

        #region Inspector

        [SerializeField]
        private Image _icon;

        [SerializeField]
        private Button _button;

        #endregion

        #region Fields

        private int _index;

        #endregion

        #region Properties

        public int Index => _index;

        #endregion

        #region Public API

        public void Initialize(int index)
        {
            _index = index;

            _button.onClick.AddListener(
                NotifyClicked);
        }

        public void SetColor(Color color)
        {
            _icon.color = color;
        }

        public void SetInteractable(bool value)
        {
            _button.interactable = value;
        }

        #endregion

        #region Internal

        private void NotifyClicked()
        {
            OnClicked?.Invoke(_index);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(
                NotifyClicked);
        }

        #endregion
    }
}