using System;
using System.Collections.Generic;
using System.Linq;
using LightHouse.Audio;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    public class OptionsMenuController : MonoBehaviour
    {
        #region FIELDS
        public event Action OnBackClicked;

        [Header("UI References")]
        [SerializeField] private UIDocument _pauseMenuDocument;
        [SerializeField] private ConfirmationPopupController _confirmationPopUpController;

        [Header("Localization Databases")]
        [SerializeField] private LocalizedStringDatabase_Options_Display _displayTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Graphisms _graphismsTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Languages _languagesTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_MouseKeyboard _mouseKeyboardTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Sounds _soundsTextsDB;

        [SerializeField] private Color _dropdownControllersBackground; //1D1D2A

        // UI Elements
        private VisualElement _rootOptions;
        private Button _applySettingsButton;
        private Button _backButton;

        // Internal State
        private Dictionary<OptionCategory, PanelData> _panelsByCategory;
        private List<OptionsNavigationButton> _optionNavigationButtons;
        private OptionCategory _currentOpenCategory = OptionCategory.Display;
        private OptionCategory _pendingTargetCategory;
        private bool _navigationPending;

        // Option Windows
        private DisplayOptionsWindow _displayOptionsWindow;
        private LanguageOptionWindow _languageOptionWindow;
        #endregion

        #region PROPERTIES
        public ConfirmationPopupController ConfirmationPopupController => _confirmationPopUpController;
        #endregion

        #region Unity Callbacks

        private void RegisterDropdowns(DropdownField dropdown)
        {
            // Attente de l'ouverture du menu dropdown
            dropdown.RegisterCallback<PointerDownEvent>(_ =>
            {
                // Petit délai pour laisser Unity créer le menu popup
                // Ensuite, on parcourt les éléments flottants
                dropdown.schedule.Execute(() =>
                {
                    var pan = _rootOptions.panel.visualTree;
                    var dropDownScroll = pan.Q<ScrollView>();
                    if (dropDownScroll == null) return;
                    dropDownScroll.style.backgroundColor = _dropdownControllersBackground;
                    dropDownScroll.style.color = Color.white;
                    var dropDownElements = dropDownScroll.Query<VisualElement>().Class("unity-base-dropdown__item");
                    List<VisualElement> elementsList = dropDownElements.ToList();

                    //Debug.Log(dropdown.choices.Count);
                    for (int i = 0; i < dropdown.choices.Count; i++)
                    {
                        if (i == dropdown.index)
                        {
                            //Debug.Log(dropdown.choices[i]);
                            //if (elementsList.Count < dropdown.choices.Count) return;
                            elementsList[i].style.color = Color.yellow;
                        }

                    }
                }).ExecuteLater(10); // 10ms environ
            });
        }

        void OnEnable()
        {

            //var dropdown = _rootOptions.Q<DropdownField>("ResolutionDropdown");
           
       /*     RegisterDropdowns(_rootOptions.Q<DropdownField>("ResolutionDropdown"));
            RegisterDropdowns(_rootOptions.Q<DropdownField>("DisplayModeDropdown"));
            RegisterDropdowns(_rootOptions.Q<DropdownField>("ResolutionDropdown"));*/
            
        }

        private void Awake()
        {
            InitializeUIReferences();
            InitializeOptionWindows();
            InitializeNavigationButtons();
            InitializePanels();
            HideAllPanels();
            SubscribeToEvents();
        }

        private void Start()
        {
            var dropdowns = _rootOptions.Query<DropdownField>().ToList();
            foreach (var dropdown in dropdowns)
            {
                RegisterDropdowns(dropdown);
            }
            UpdateAllTextsLanguage();
            NavigateTo(OptionCategory.Display, forcePerform: true);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            DisposeNavigationButtons();
        }

        #endregion

        #region Initialization

        private void InitializeUIReferences()
        {
            _rootOptions = _pauseMenuDocument.rootVisualElement.Q<VisualElement>("Options_Root");

            _applySettingsButton = _rootOptions.Q<Button>("ApplyButton");
            _backButton = _rootOptions.Q<Button>("BackButton");

            _applySettingsButton.clicked += OnApplySettingsClicked;
            _backButton.clicked += OnBackButtonClicked;

            var allButtons = _rootOptions.Query<Button>().ToList();
            foreach(var btn in allButtons)
            {
                btn.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    if (AudioHandlerData.AudioManager != null)
                        AudioHandlerData.AudioManager.PlayRandomEffect(this.transform, Audio.AutoGenerated.EffectsAudioName.Effect_ButtonHover);
                });
                btn.RegisterCallback<ClickEvent>(evt =>
                {
                    if (AudioHandlerData.AudioManager != null)
                        AudioHandlerData.AudioManager.PlayRandomEffect(this.transform, Audio.AutoGenerated.EffectsAudioName.Effect_ButtonCliqued);
                });
            }
        }

        private void InitializeOptionWindows()
        {
            _displayOptionsWindow = new DisplayOptionsWindow(_rootOptions, _confirmationPopUpController, _displayTextsDB);
            _languageOptionWindow = new LanguageOptionWindow(_rootOptions, _confirmationPopUpController, _languagesTextsDB);
        }

        private void InitializeNavigationButtons()
        {
            _optionNavigationButtons = new List<OptionsNavigationButton>
            {
                new OptionsNavigationButton(_rootOptions.Q<Button>("DisplayButton"), OptionCategory.Display, this, _displayTextsDB.Section_Name),
                new OptionsNavigationButton(_rootOptions.Q<Button>("GraphicsButton"), OptionCategory.Graphics, this, _graphismsTextsDB.Section_Name),
                new OptionsNavigationButton(_rootOptions.Q<Button>("AudioButton"), OptionCategory.Audio, this, _soundsTextsDB.Section_Name),
                new OptionsNavigationButton(_rootOptions.Q<Button>("LanguageButton"), OptionCategory.Language, this, _languagesTextsDB.Section_Name),
                new OptionsNavigationButton(_rootOptions.Q<Button>("InputButton"), OptionCategory.Input, this, _mouseKeyboardTextsDB.Section_Name),
            };
        }

        private void InitializePanels()
        {
            _panelsByCategory = new Dictionary<OptionCategory, PanelData>
            {
                { OptionCategory.Display, new PanelData(_rootOptions.Q<VisualElement>("DisplayPanel"), _displayOptionsWindow) },
                { OptionCategory.Graphics, new PanelData(_rootOptions.Q<VisualElement>("GraphicsPanel"), null) },
                { OptionCategory.Audio, new PanelData(_rootOptions.Q<VisualElement>("AudioPanel"), null) },
                { OptionCategory.Language, new PanelData(_rootOptions.Q<VisualElement>("LanguagePanel"), _languageOptionWindow) },
                { OptionCategory.Input, new PanelData(_rootOptions.Q<VisualElement>("InputPanel"), null) }
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

            _applySettingsButton.clicked -= OnApplySettingsClicked;
            _backButton.clicked -= OnBackButtonClicked;
        }

        private void DisposeNavigationButtons()
        {
            foreach (var navButton in _optionNavigationButtons)
            {
                navButton.Dispose();
            }
        }

        #endregion
        private OptionsNavigationButton _lastSelectedButton;

        public void HighlightSelectedButton(OptionsNavigationButton selected, bool value)
        {
            if(_lastSelectedButton != null)
            {
                _lastSelectedButton.SetSelected(false);
            }
            _lastSelectedButton = selected;
            selected.SetSelected(value);
        }

        #region UI Updates

        private void UpdateAllTextsLanguage()
        {
            foreach (var button in _optionNavigationButtons)
            {
                button.UpdateLocalizedText();
            }

            _displayOptionsWindow.UpdateAllTextsLanguage();
            _languageOptionWindow.UpdateAllTextsLanguage();
        }

        private void RefreshDisplayOptionsUI()
        {
            _displayOptionsWindow.RefreshOnlyUI();
        }

        private void HideAllPanels()
        {
            foreach (var panelData in _panelsByCategory.Values)
            {
                panelData.Panel.SetDisplayStyle(DisplayStyle.None);
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
            UpdateAllTextsLanguage();
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
            _panelsByCategory[_currentOpenCategory].Window?.ApplySettings();
        }

        private void ApplyCanceled()
        {
            _panelsByCategory[_currentOpenCategory].Window?.RevertSettings();
        }

        #endregion

        #region Navigation

        public void NavigateTo(OptionCategory category, bool forcePerform = false)
        {
            if (!forcePerform && _panelsByCategory[_currentOpenCategory].Window?.HasChanges() == true)
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

            if (_panelsByCategory.TryGetValue(category, out var panelData) && panelData.Panel != null)
            {
                panelData.Panel.SetDisplayStyle(DisplayStyle.Flex);
                _currentOpenCategory = category;
            }
        }


        private void OnConfirmNavigation()
        {
            _panelsByCategory[_currentOpenCategory].Window?.ApplySettings();

            if (_navigationPending)
            {
                _navigationPending = false;
                PerformNavigation(_pendingTargetCategory);
            }
        }

        private void OnCancelNavigation()
        {
            _panelsByCategory[_currentOpenCategory].Window?.RevertSettings();

            if (_navigationPending)
            {
                _navigationPending = false;
                PerformNavigation(_pendingTargetCategory);
            }
        }

        #endregion
    }

    internal static class VisualElementExtensions
    {
        public static void SetDisplayStyle(this VisualElement element, DisplayStyle style)
        {
            if (element != null)
                element.style.display = style;
        }
    }
}
