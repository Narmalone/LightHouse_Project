using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Single selectable color button.
    /// </summary>
    public class PlayerGuessColorButtonView :
        MonoBehaviour
    {
        #region Events

        public event Action<MastermindColor>
            OnClicked;

        #endregion

        #region Inspector

        [SerializeField]
        private Button _button;

        [SerializeField]
        private Image _icon;

        [SerializeField]
        private Outline _selectionOutline;

        #endregion

        #region Properties

        public MastermindColor Color
        { get; private set; }

        #endregion

        #region Public API

        public void Initialize(
            MastermindColor color,
            Color32 visualColor)
        {
            Color = color;

            _icon.color = visualColor;

            _button.onClick.AddListener(
                NotifyClicked);

            SetSelected(false);
        }

        public void SetSelected(bool value)
        {
            if (_selectionOutline != null)
            {
                _selectionOutline.enabled = value;
            }
        }

        #endregion

        #region Internal

        private void NotifyClicked()
        {
            OnClicked?.Invoke(Color);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(
                NotifyClicked);
        }

        #endregion
    }
}