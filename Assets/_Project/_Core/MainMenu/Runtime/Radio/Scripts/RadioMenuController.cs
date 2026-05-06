using Cinemachine;
using LightHouse.Core.Interaction;
using UnityEngine;

namespace LightHouse.Features.Menu.Radio
{
    /// <summary>
    /// Gère les interactions avec la radio dans le menu (entrée, sortie, activation, caméra).
    /// </summary>
    public class RadioMenuController : MonoBehaviour, IClickable
    {
        #region ===== Dependencies =====

        [SerializeField] private BoxCollider _mainCollider;
        [SerializeField] private RadioDial _radioDial;
        [SerializeField] private RadioSystemBinder _radioBinder;
        [SerializeField] private RadioFrequencyController _frequencyController;
        [SerializeField] private RadioAudioController _radioAudioController;
        [SerializeField] private RadioOnOffController _radioOnOffController;
        [SerializeField] private CinemachineVirtualCamera _radioCamera;

        #endregion

        #region ===== State =====

        private bool _isPlayerInsideTheRadio = false;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            SubscribeEvents();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            _radioCamera.Priority = -1;

            DisableRadio();
            LeaveRadio();
        }

        #endregion

        #region ===== Events =====

        private void SubscribeEvents()
        {
            _radioOnOffController.Toggle.OnValueChanged += OnToggleChanged;
            PlayerControllerMenu.OnRightClickPressed += OnRightClickPressed;
        }

        private void UnsubscribeEvents()
        {
            _radioOnOffController.Toggle.OnValueChanged -= OnToggleChanged;
            PlayerControllerMenu.OnRightClickPressed -= OnRightClickPressed;
        }

        private void OnRightClickPressed()
        {
            if (!_isPlayerInsideTheRadio) return;

            LeaveRadio();
        }

        private void OnToggleChanged(bool isOn)
        {
            if (isOn)
                EnableRadio();
            else
                DisableRadio();
        }

        #endregion

        #region ===== Radio Logic =====

        private void EnableRadio()
        {
            _radioBinder.SetEnable(true);
            _radioAudioController.PlayAudio();
            _frequencyController.ShowFrequencyText();
            _radioDial.ForceUpdateValue();
        }

        private void DisableRadio()
        {
            _radioBinder.SetEnable(false);
            _radioAudioController.StopAudio();
            _frequencyController.HideFrequencyText();
        }

        #endregion

        #region ===== Interaction =====

        private void EnterRadio()
        {
            _isPlayerInsideTheRadio = true;
            _mainCollider.enabled = false;
            _radioCamera.Priority = 100;
        }

        private void LeaveRadio()
        {
            _isPlayerInsideTheRadio = false;
            _mainCollider.enabled = true;
            _radioCamera.Priority = -1;
        }

        #endregion

        #region ===== Interface =====

        public void OnClicked()
        {
            EnterRadio();
        }

        #endregion
    }
}