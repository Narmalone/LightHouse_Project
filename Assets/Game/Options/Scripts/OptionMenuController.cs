using System;
using System.Collections.Generic;
using System.Linq;
using LightHouse.Handlers;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{

    public class OptionsMenuController : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument optionsDocument;
        [SerializeField] public ConfirmationPopupController confirmationPopupController;
        [SerializeField] private LocalizedStringDatabase_Options_Display _displayTextsDB;
        [SerializeField] private LocalizedStringDatabase_Options_Languages _languagesTextsDB;

        private VisualElement _rootOptions;

        private Dictionary<OptionCategory, PanelData> panelsByCategory;
        private OptionCategory currentCategory = OptionCategory.Display;

        private DisplayOptionsWindow displayOptionsWindow;
        private LanguageOptionWindow languageOptionWindow;

        private List<OptionsNavigationButton> navigationButtons;

        public event Action OnBackCliqued;
        private Button _applySttingsBtn;

        private Button backbutton;

        private OptionCategory pendingTargetCategory;
        private bool navigationPending;

        private void Awake()
        {
            _rootOptions = optionsDocument.rootVisualElement.Q<VisualElement>("Root_Options");

            _applySttingsBtn = _rootOptions.Q<Button>("ApplyButton");
            backbutton = _rootOptions.Q<Button>("BackButton");
            backbutton.clicked += Backbutton_clicked;
            _applySttingsBtn.clicked += ApplySettingsCliqued;

            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;

            DisplaysSetting.OnDisplayScreenChanged += DisplaysSetting_OnDisplayScreenChanged;

            displayOptionsWindow = new DisplayOptionsWindow(_rootOptions, confirmationPopupController, _displayTextsDB);
            languageOptionWindow = new LanguageOptionWindow(_rootOptions, confirmationPopupController, _languagesTextsDB);

            DisplaySettingManager.OnDisplayChanged += RefreshDisplayOptionsUI;

            InitializePanels();
            InitializeNavigationButtons();
            HideAllPanels();

        }

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
            confirmationPopupController.Show(ApplyConfirmed, ApplyCanceled);
        }

        private void ApplyCanceled()
        {
            //cancel change on the current pannel
            panelsByCategory[currentCategory].Window.RevertSettings();
        }

        private void ApplyConfirmed()
        {
            //Apply changes on the current pannel
            panelsByCategory[currentCategory].Window.ApplySettings();
        }

        private void OnDestroy()
        {
            foreach(OptionsNavigationButton navButton in navigationButtons)
            {
                navButton.Dispose();
            }
            _applySttingsBtn.clicked -= ApplySettingsCliqued;
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
            DisplaySettingManager.OnDisplayChanged -= RefreshDisplayOptionsUI;
            backbutton.clicked -= Backbutton_clicked;
        }

        private void LocalizationSettings_SelectedLocaleChanged(UnityEngine.Localization.Locale obj)
        {
            UpdateAllTextsLanguage();
        }

        private void InitializeNavigationButtons()
        {
            navigationButtons = new List<OptionsNavigationButton>
            {
                 new OptionsNavigationButton(_rootOptions.Q<Button>("DisplayButton"), OptionCategory.Display, this, _displayTextsDB.Section_Name),
                 new OptionsNavigationButton(_rootOptions.Q<Button>("GraphicsButton"), OptionCategory.Graphics, this, null),
                 new OptionsNavigationButton(_rootOptions.Q<Button>("AudioButton"), OptionCategory.Audio, this, null),
                 new OptionsNavigationButton(_rootOptions.Q<Button>("LanguageButton"), OptionCategory.Language, this, _languagesTextsDB.Section_Name),
                 new OptionsNavigationButton(_rootOptions.Q<Button>("InputButton"), OptionCategory.Input, this, null),
            };
        }

        private void UpdateAllTextsLanguage()
        {
            foreach (var button in navigationButtons) 
            {
                button.UpdateLocalizedText();    
            }
            displayOptionsWindow.UpdateAllTextsLanguage();
            languageOptionWindow.UpdateAllTextsLanguage();

        }

        private void Start()
        {
            UpdateAllTextsLanguage();
            NavigateTo(OptionCategory.Display, true);
        }

        private void InitializePanels()
        {
            panelsByCategory = new Dictionary<OptionCategory, PanelData>
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
            foreach (var pannelData in panelsByCategory.Values)
            {
                if (pannelData != null && pannelData.Panel != null)
                    pannelData.Panel.style.display = DisplayStyle.None;
            }
        }

        public void NavigateTo(OptionCategory category, bool forcePerform = false)
        {
            if (!forcePerform)
            {
                OptionWindowBase currentWindow = panelsByCategory[currentCategory].Window;
                if (currentWindow != null)
                {
                    if (currentWindow.HasChanges())
                    {
                        pendingTargetCategory = category;
                        navigationPending = true;
                        confirmationPopupController.Show(OnConfirmNavigation, OnCancelNavigation);
                        return;
                    }
                }
            }
            // Sinon navigation directe
            PerformNavigation(category);
        }

        private void PerformNavigation(OptionCategory category)
        {
            HideAllPanels();

            if (panelsByCategory.TryGetValue(category, out var pannelData) && pannelData.Panel != null)
            {
                pannelData.Panel.style.display = DisplayStyle.Flex;
                currentCategory = category;
            }
            else
            {
                Debug.LogWarning($"[OptionsMenuController] Aucun panel trouvé pour la catégorie {category}");
            }
        }

        private void OnConfirmNavigation()
        {
            OptionWindowBase currentWindow = panelsByCategory[currentCategory].Window;
            currentWindow?.ApplySettings();

            if (navigationPending)
            {
                navigationPending = false;
                PerformNavigation(pendingTargetCategory);
            }
        }

        private void OnCancelNavigation()
        {
            OptionWindowBase currentWindow = panelsByCategory[currentCategory].Window;
            currentWindow?.RevertSettings();

            if (navigationPending)
            {
                navigationPending = false;
                PerformNavigation(pendingTargetCategory);
            }
        }
    }
}
