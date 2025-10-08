using System;
using System.Collections.Generic;
using System.Linq;
using LightHouse.Localization;
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
        [SerializeField] private DisplayOptionsWindow _displayOptionsWindow;
        [SerializeField] private AudioOptionsWindow _audioOptionsWindow;
        private LanguageOptionWindow _languageOptionWindow;
        #endregion

        #region PROPERTIES
        public ConfirmationPopupController ConfirmationPopupController => _confirmationPopUpController;
        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            InitializeUIReferences();
            InitializePanels();
            HideAllPanels();
            SubscribeToEvents();
        }

        private void Start()
        {
            
            NavigateTo(OptionCategory.Video, forcePerform: true);
        }

        private void OnValidate()
        {
            _optionNavigationButtons = GetComponentsInChildren<OptionsNavigationButton>().ToList();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

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
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            DisplaysSetting.OnDisplayScreenChanged += OnDisplayScreenChanged;
            DisplaySettingManager.OnDisplayChanged += RefreshDisplayOptionsUI;
        }

        private void UnsubscribeFromEvents()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            DisplaysSetting.OnDisplayScreenChanged -= OnDisplayScreenChanged;
            DisplaySettingManager.OnDisplayChanged -= RefreshDisplayOptionsUI;

            _applySettingsButton.onClick.RemoveListener(OnApplySettingsClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);

            foreach (var btn in _optionNavigationButtons)
            {
                btn.OnCliqued -= Btn_OnCliqued;
            }
        }

        #endregion

        #region UI Updates

        private void RefreshDisplayOptionsUI()
        {
            _displayOptionsWindow.RefreshOnlyUI();
        }

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
            if (DisplaysSetting.IsRevertingDisplay)
            {
                Debug.Log("[OptionsMenuController] Display reverted, skipping reinitialization.");
                return;
            }

            _displayOptionsWindow.InitializeControllers();
            _displayOptionsWindow.ApplySettings();
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
