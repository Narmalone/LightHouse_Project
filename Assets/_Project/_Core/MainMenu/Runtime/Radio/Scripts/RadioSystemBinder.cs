using LightHouse.Core.Interaction;
using LightHouse.Core.Interaction.Feedback;
using UnityEngine;

namespace LightHouse.Features.Menu.Radio
{
    /// <summary>
    /// Relie les systèmes de la radio (interaction, logique, feedback visuel et audio).
    /// </summary>
    public class RadioSystemBinder : MonoBehaviour
    {
        #region ===== Dependencies =====

        [Header("Core")]
        [SerializeField] private InteractableBase _interactable;
        [SerializeField] private RadioDial _dial;
        [SerializeField] private RadioFrequencyController _frequencyController;

        [Header("Feedback")]
        [SerializeField] private AudioFeedback _audioFeedback;
        [SerializeField] private EmissiveFeedback _emissiveFeedback;

        #endregion

        #region ===== Settings =====

        [SerializeField] private bool _enableActiveState = false;

        #endregion

        #region ===== State =====

        private bool _isActive;
        private bool _isEnabled;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            BindCore();
            BindFeedback();
            BindInteraction();
        }

        private void OnDestroy()
        {
            UnbindCore();
            UnbindInteraction();
        }

        #endregion

        #region ===== Binding =====

        private void BindCore()
        {
            if (_dial != null && _frequencyController != null)
                _dial.OnValueChanged += _frequencyController.SetFrequency;
        }

        private void UnbindCore()
        {
            if (_dial != null && _frequencyController != null)
                _dial.OnValueChanged -= _frequencyController.SetFrequency;
        }

        private void BindFeedback()
        {
            if (_audioFeedback != null)
                _audioFeedback.Bind(_interactable);
        }

        private void BindInteraction()
        {
            if (_interactable == null) return;

            _interactable.OnHoverEnter += OnHoverEnter;
            _interactable.OnHoverExit += OnHoverExit;
            _interactable.OnClickDown += OnClick;
        }

        private void UnbindInteraction()
        {
            if (_interactable == null) return;

            _interactable.OnHoverEnter -= OnHoverEnter;
            _interactable.OnHoverExit -= OnHoverExit;
            _interactable.OnClickDown -= OnClick;
        }

        #endregion

        #region ===== Interaction =====

        private void OnHoverEnter()
        {
            if (!IsInteractable()) return;

            ApplyState(EmissiveState.Hover);
        }

        private void OnHoverExit()
        {
            if (!IsInteractable()) return;

            ApplyState(GetIdleState());
        }

        private void OnClick()
        {
            if (!IsInteractable() || !_enableActiveState) return;

            ToggleActive();
            ApplyState(GetIdleState());
        }

        #endregion

        #region ===== State Logic =====

        private bool IsInteractable()
        {
            return _isEnabled;
        }

        private void ToggleActive()
        {
            _isActive = !_isActive;
        }

        private EmissiveState GetIdleState()
        {
            return _isActive
                ? EmissiveState.Active
                : EmissiveState.Default;
        }

        private void ApplyState(EmissiveState state)
        {
            _emissiveFeedback?.ApplyState(state);
        }

        #endregion

        #region ===== External Control =====

        /// <summary>
        /// Active ou désactive complètement le système radio (appelé par ON/OFF).
        /// </summary>
        public void SetEnable(bool enable)
        {
            _isEnabled = enable;

            _audioFeedback?.SetEnable(enable);

            if (!_isEnabled)
            {
                ApplyState(EmissiveState.Disabled);
                return;
            }

            ApplyState(GetIdleState());
        }

        #endregion
    }
}