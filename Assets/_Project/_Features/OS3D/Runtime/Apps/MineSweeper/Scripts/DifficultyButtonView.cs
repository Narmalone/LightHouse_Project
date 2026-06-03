using System;
using TMPro;
using UnityEngine;
using LightHouse.Features.UI;

namespace LightHouse.Features.Computer
{
    public class DifficultyButtonView :
        MonoBehaviour
    {
        #region Events

        public event Action OnClicked;

        public event Action OnHovered;

        #endregion

        #region Inspector

        [SerializeField]
        private UI_CustomButton
            _button;

        [SerializeField]
        private TextMeshProUGUI
            _titleText;

        #endregion

        #region Public API

        public void Initialize(
            string buttonName)
        {
            _titleText.text =
                buttonName.ToUpper();

            _button.OnClick +=
                HandleClicked;

            _button.OnHoverEnter +=
                HandleHovered;
        }

        #endregion

        #region Internal

        private void HandleClicked(
            UI_CustomButton button)
        {
            OnClicked?.Invoke();
        }

        private void HandleHovered(
            UI_CustomButton button)
        {
            OnHovered?.Invoke();
        }

        private void OnDestroy()
        {
            _button.OnClick -=
                HandleClicked;

            _button.OnHoverEnter -=
                HandleHovered;
        }

        #endregion
    }
}