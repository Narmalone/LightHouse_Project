using LightHouse.Handlers;
using LightHouse.KinematicCharacterController;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LightHouse.Game.Options
{
    public class OptionsMenuController : MonoBehaviour
    {
        #region FIELDS
        public event Action OnBackClicked;

        [Header("UI References")]
        //[SerializeField] private UIDocument _pauseMenuDocument;
        [SerializeField] private ConfirmationPopupController _confirmationPopUpController;
        [SerializeField] private CanvasGroup _optionCanvasGroup;

        // UI Elements
        [SerializeField] private Button _applySettingsButton;
        [SerializeField] private Button _backButton;

        // Internal State
        public Dictionary<OptionCategory, OptionWindowBase> _panelsByCategory;
        [SerializeField] private List<OptionsNavigationButton> _optionNavigationButtons;
        private OptionCategory _currentOpenCategory = OptionCategory.Video;
        private OptionCategory _pendingTargetCategory;
        private bool _navigationPending;

        // Option Windows
        [SerializeField] private VideoOptionsController _displayOptionsWindow;
        [SerializeField] private AudioOptionsWindow _audioOptionsWindow;
        #endregion

        #region PROPERTIES
        public ConfirmationPopupController ConfirmationPopupController => _confirmationPopUpController;
        public bool IsEnabled { get; private set; } = false;
        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _optionNavigationButtons = GetComponentsInChildren<OptionsNavigationButton>().ToList();
            InitializeUIReferences();
            InitializePanels();
            HideAllPanels();
            SubscribeToEvents();
        }

        private void Start()
        {
            NavigateTo(OptionCategory.Video, forcePerform: true);
            Disable();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (IsEnabled)
                    Disable();
                else Enable();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        public void Enable()
        {
            Player.ForceChangePlayerState.Invoke(PlayerState.Options);
            IsEnabled = true;
            _optionCanvasGroup.interactable = true;
            _optionCanvasGroup.alpha = 1.0f;
            _optionCanvasGroup.blocksRaycasts = true;
        }

        public void Disable()
        {
            if(PlayerHandlerData.MainPlayer != null)
            {
                if (PlayerHandlerData.MainPlayer.PlayerState == PlayerState.ComputerMode)
                    Player.ForceChangePlayerState?.Invoke(PlayerState.ComputerMode);
                else
                    PlayerHandlerData.MainPlayer.RevertToPreviousState();
            }
            IsEnabled = false;
            _optionCanvasGroup.interactable = false;
            _optionCanvasGroup.alpha = 0.0f;
            _optionCanvasGroup.blocksRaycasts = false;
        }

        #region Initialization

        private void InitializeUIReferences()
        {
            _applySettingsButton.onClick.AddListener(OnApplySettingsClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);

            foreach(var btn in _optionNavigationButtons)
            {
                btn.OnCliqued += Btn_OnCliqued;
            }
        }

        private void Btn_OnCliqued(OptionsNavigationButton obj)
        {
            NavigateTo(obj.TargetCategory);
        }

        private void InitializePanels()
        {
            _panelsByCategory = new Dictionary<OptionCategory, OptionWindowBase>
            {
                { OptionCategory.Video, _displayOptionsWindow },
                { OptionCategory.Audio, _audioOptionsWindow }
               /* { OptionCategory.Graphics,  },
                { OptionCategory.Audio, },
                { OptionCategory.Language, },
                { OptionCategory.Input, }*/
            };
        }

        private void SubscribeToEvents()
        {

        }

        private void UnsubscribeFromEvents()
        {
            _applySettingsButton.onClick.RemoveListener(OnApplySettingsClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);

            foreach (var btn in _optionNavigationButtons)
            {
                btn.OnCliqued -= Btn_OnCliqued;
            }
        }

        #endregion

        #region UI Updates
        private void HideAllPanels()
        {
            foreach (var panelData in _panelsByCategory.Values)
            {
                panelData.Hide();
            }
        }

        #endregion

        #region Event Handlers

        private void OnApplySettingsClicked()
        {
            _confirmationPopUpController.Show(ApplyConfirmed, ApplyCanceled);
        }

        private void OnBackButtonClicked()
        {
            OnBackClicked?.Invoke();
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale obj)
        {
            
        }

        private void OnDisplayScreenChanged()
        {
           /* if (DisplaysSetting.IsRevertingDisplay)
            {
                Debug.Log("[OptionsMenuController] Display reverted, skipping reinitialization.");
                return;
            }

            _displayOptionsWindow.InitializeControllers();
            _displayOptionsWindow.ApplySettings();*/
        }

        #endregion

        #region Apply / Cancel

        private void ApplyConfirmed()
        {
            _panelsByCategory[_currentOpenCategory]?.ApplySettings();
        }

        private void ApplyCanceled()
        {
            _panelsByCategory[_currentOpenCategory]?.RevertSettings();
        }

        #endregion

        #region Navigation

        public void NavigateTo(OptionCategory category, bool forcePerform = false)
        {
            if (!forcePerform && _panelsByCategory[_currentOpenCategory]?.HasChanges() == true)
            {
                _pendingTargetCategory = category;
                _navigationPending = true;
                _confirmationPopUpController.Show(OnConfirmNavigation, OnCancelNavigation);
                return;
            }

            PerformNavigation(category);
        }

        private void PerformNavigation(OptionCategory category)
        {
            HideAllPanels();

            if (_panelsByCategory.TryGetValue(category, out var panelData) && panelData != null)
            {
                panelData.Show();
                _currentOpenCategory = category;
            }
        }

        private void OnConfirmNavigation()
        {
            _panelsByCategory[_currentOpenCategory]?.ApplySettings();

            if (_navigationPending)
            {
                _navigationPending = false;
                PerformNavigation(_pendingTargetCategory);
            }
        }

        private void OnCancelNavigation()
        {
            _panelsByCategory[_currentOpenCategory]?.RevertSettings();

            if (_navigationPending)
            {
                _navigationPending = false;
                PerformNavigation(_pendingTargetCategory);
            }
        }

        #endregion
    }
}
