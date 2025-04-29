using System;
using System.Collections.Generic;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{

    public class OptionsMenuController : MonoBehaviour
    {
        public event Action OnBackCliqued;

        [Header("UI Document")]
        [SerializeField] private UIDocument _pauseMenuDocument;
        [SerializeField] public ConfirmationPopupController _confirmationPopUpController;
        [SerializeField] private LocalizedStringDatabase_Options_Display _displayTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Graphisms _graphismsTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Languages _languagesTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_MouseKeyboard _mouseKeyboardTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Sounds _soundsTextsDB;

        private Dictionary<OptionCategory, PanelData> _pannelsByCategory;
        private OptionCategory _currentOpenCategory = OptionCategory.Display;
        //Navigation Buttons
        private List<OptionsNavigationButton> _optionNavigationButtons;

        //Option Controllers
        private DisplayOptionsWindow displayOptionsWindow;
        private LanguageOptionWindow languageOptionWindow;

        private VisualElement _rootOptions;
        private Button _applySettingsButton; //to doo transformer en LocalizedButton
        private Button _optionToPauseButton; //to do transformer en localized button

        private OptionCategory pendingTargetCategory;
        private bool navigationPending;

        #region MONO CALLBACKS
        private void Awake()
        {
            _rootOptions = _pauseMenuDocument.rootVisualElement.Q<VisualElement>("Root_Options");

            _applySettingsButton = _rootOptions.Q<Button>("ApplyButton");
            _optionToPauseButton = _rootOptions.Q<Button>("BackButton");
            _optionToPauseButton.clicked += Backbutton_clicked;
            _applySettingsButton.clicked += ApplySettingsCliqued;

            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;

            DisplaysSetting.OnDisplayScreenChanged += DisplaysSetting_OnDisplayScreenChanged;

            displayOptionsWindow = new DisplayOptionsWindow(_rootOptions, _confirmationPopUpController, _displayTextsDB);
            languageOptionWindow = new LanguageOptionWindow(_rootOptions, _confirmationPopUpController, _languagesTextsDB);

            DisplaySettingManager.OnDisplayChanged += RefreshDisplayOptionsUI;

            InitializePanels();
            InitializeNavigationButtons();
            HideAllPanels();

        }
        private void Start()
        {
            UpdateAllTextsLanguage();
            NavigateTo(OptionCategory.Display, true);
        }
        #endregion

        private void Backbutton_clicked()
        {
            OnBackCliqued?.Invoke();
        }

        private void RefreshDisplayOptionsUI()
        {
            displayOptionsWindow.RefreshOnlyUI();
        }

        private void DisplaysSetting_OnDisplayScreenChanged()
        {
            if (DisplaysSetting.IsRevertingDisplay)
            {
                Debug.Log("Display reverted, pas de reinitialisation !");
                return;
            }

            displayOptionsWindow.InitializeControllers();
            displayOptionsWindow.ApplySettings();
        }


        private void ApplySettingsCliqued()
        {
            _confirmationPopUpController.Show(ApplyConfirmed, ApplyCanceled);
        }

        private void ApplyCanceled()
        {
            //cancel change on the current pannel
            _pannelsByCategory[_currentOpenCategory].Window.RevertSettings();
        }

        private void ApplyConfirmed()
        {
            //Apply changes on the current pannel
            _pannelsByCategory[_currentOpenCategory].Window.ApplySettings();
        }

        private void OnDestroy()
        {
            foreach(OptionsNavigationButton navButton in _optionNavigationButtons)
            {
                navButton.Dispose();
            }
            _applySettingsButton.clicked -= ApplySettingsCliqued;
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
            DisplaySettingManager.OnDisplayChanged -= RefreshDisplayOptionsUI;
            _optionToPauseButton.clicked -= Backbutton_clicked;
        }

        private void LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Locale obj)
        {
            UpdateAllTextsLanguage();
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

        private void UpdateAllTextsLanguage()
        {
            foreach (var button in _optionNavigationButtons) 
            {
                button.UpdateLocalizedText();    
            }
            displayOptionsWindow.UpdateAllTextsLanguage();
            languageOptionWindow.UpdateAllTextsLanguage();

        }

        private void InitializePanels()
        {
            _pannelsByCategory = new Dictionary<OptionCategory, PanelData>
            {
                { OptionCategory.Display, new PanelData(_rootOptions.Q<VisualElement>("DisplayPanel"), displayOptionsWindow) },
                { OptionCategory.Graphics, new PanelData(_rootOptions.Q<VisualElement>("GraphicsPanel"), null) },
                { OptionCategory.Audio, new PanelData(_rootOptions.Q<VisualElement>("AudioPanel"), null) },
                { OptionCategory.Language, new PanelData(_rootOptions.Q<VisualElement>("LanguagePanel"), languageOptionWindow) },
                { OptionCategory.Input, new PanelData(_rootOptions.Q<VisualElement>("InputPanel"), null) }
            };
        }


        private void HideAllPanels()
        {
            foreach (var pannelData in _pannelsByCategory.Values)
            {
                if (pannelData != null && pannelData.Panel != null)
                    pannelData.Panel.style.display = DisplayStyle.None;
            }
        }

        #region NAVIGATION
        public void NavigateTo(OptionCategory category, bool forcePerform = false)
        {
            if (!forcePerform)
            {
                OptionWindowBase currentWindow = _pannelsByCategory[_currentOpenCategory].Window;
                if (currentWindow != null)
                {
                    if (currentWindow.HasChanges())
                    {
                        pendingTargetCategory = category;
                        navigationPending = true;
                        _confirmationPopUpController.Show(OnConfirmNavigation, OnCancelNavigation);
                        return;
                    }
                }
            }
            // else direct navigation
            PerformNavigation(category);
        }

        private void PerformNavigation(OptionCategory category)
        {
            HideAllPanels();

            if (_pannelsByCategory.TryGetValue(category, out var pannelData) && pannelData.Panel != null)
            {
                pannelData.Panel.style.display = DisplayStyle.Flex;
                _currentOpenCategory = category;
            }
            else
            {
                Debug.LogWarning($"[OptionsMenuController] Aucun panel trouvé pour la catégorie {category}");
            }
        }

        private void OnConfirmNavigation()
        {
            OptionWindowBase currentWindow = _pannelsByCategory[_currentOpenCategory].Window;
            currentWindow?.ApplySettings();

            if (navigationPending)
            {
                navigationPending = false;
                PerformNavigation(pendingTargetCategory);
            }
        }

        private void OnCancelNavigation()
        {
            OptionWindowBase currentWindow = _pannelsByCategory[_currentOpenCategory].Window;
            currentWindow?.RevertSettings();

            if (navigationPending)
            {
                navigationPending = false;
                PerformNavigation(pendingTargetCategory);
            }
        }
        #endregion
    }
}
