using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Bottom palette board where the player selects colors.
    /// </summary>
    public class PlayerGuessColorsBoardView : MonoBehaviour
    {
        #region Inspector

        [Header("References")]

        [SerializeField]
        private PlayerGuessColorButtonView _colorButtonPrefab;

        [SerializeField]
        private Transform _buttonsContainer;

        [Header("Runtime")]

        [SerializeField]
        private List<PlayerGuessColorButtonView> _buttons =
            new();

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the player selects a color.
        /// The GameController listens to this event.
        /// </summary>
        public System.Action<MastermindColor>
            OnColorSelected;

        #endregion

        #region Public API

        public void Initialize(
            MastermindSettings settings)
        {
            ClearButtons();

            // Generate one button per available color
            // defined inside the settings.
            for (int i = 0;
                 i < settings.AvailableColors.Length;
                 i++)
            {
                // Current gameplay color
                // Example : Red / Blue / Yellow...
                MastermindColor color =
                    settings.AvailableColors[i];

                // Instantiate the UI button.
                PlayerGuessColorButtonView button =
                    Instantiate(
                        _colorButtonPrefab,
                        _buttonsContainer);

                // Setup:
                // - gameplay color
                // - visual color shown in UI
                button.Initialize(
                    color,
                    settings.GetVisualColor(color));

                // Subscribe to the button click.
                // When clicked:
                // -> HandleColorClicked(color)
                button.OnClicked += HandleColorClicked;

                _buttons.Add(button);
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Called when the player clicks a color button.
        /// </summary>
        private void HandleColorClicked(
            MastermindColor color)
        {
            // Notify the GameController
            // which color is now selected.
            OnColorSelected?.Invoke(color);

            // Refresh visuals:
            // selected button gets highlighted,
            // all others get unhighlighted.
            UpdateSelectionVisual(color);
        }

        /// <summary>
        /// Updates which palette button is visually selected.
        /// </summary>
        private void UpdateSelectionVisual(
            MastermindColor selectedColor)
        {
            // Loop through ALL palette buttons.
            for (int i = 0;
                 i < _buttons.Count;
                 i++)
            {
                // Is THIS button the selected one ?
                bool isSelected =
                    _buttons[i].Color ==
                    selectedColor;

                // If true:
                // -> enable outline/glow
                //
                // If false:
                // -> disable outline/glow
                _buttons[i].SetSelected(isSelected);
            }
        }

        /// <summary>
        /// Destroys previously generated buttons.
        /// Useful when reinitializing the board.
        /// </summary>
        private void ClearButtons()
        {
            for (int i = 0;
                 i < _buttons.Count;
                 i++)
            {
                if (_buttons[i] == null)
                    continue;

                // Unsubscribe event before destroy
                // to avoid dangling listeners.
                _buttons[i].OnClicked -=
                    HandleColorClicked;

                Destroy(_buttons[i].gameObject);
            }

            _buttons.Clear();
        }

        #endregion
    }
}