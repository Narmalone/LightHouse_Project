using UnityEngine.UIElements;
using LightHouse.Localization;
using System;
using UnityEngine;

namespace LightHouse.Game.Options
{
    public class DisplayOptionsWindow : OptionWindowBase
    {
        #region FIELDS
        public NavigationButton NavButton;

        protected LocalizedStringDatabase_Options_Display _localizedDB;
        private ResolutionDropdownController resolutionController;
        private DisplaysDropdownController displaysController;
        private DisplayModeDropdownController displayModeController;
        private VSyncToggleController vsyncToggleController;
        private RefreshRateDropdownController refreshRateDropdownController;
        private FrameRateDropdownController frameRateDropdownController;
        #endregion

        public DisplayOptionsWindow(VisualElement root, ConfirmationPopupController confirmationPopup, LocalizedStringDatabase_Options_Display optionDB) : base(root, confirmationPopup)
        {
            _localizedDB = optionDB;
            GenerateClasses();
            InitializeControllers();
        }

        public void UpdateAllTextsLanguage()
        {
            resolutionController.UpdateLanguage();
            displaysController.UpdateLanguage();
            displayModeController.UpdateLanguage();
            vsyncToggleController.UpdateLanguage();
            refreshRateDropdownController.UpdateLanguage();
            frameRateDropdownController.UpdateLanguage();
        }

        public void GenerateClasses()
        {
            resolutionController = new ResolutionDropdownController
            (
                root.Q<DropdownField>("ResolutionDropdown"),
                new ResolutionSetting(),
                optionDisplayDB: _localizedDB
            );

            displaysController = new DisplaysDropdownController
            (
                root.Q<DropdownField>("DisplaysDropdown"),
                confirmationPopupController,
                _localizedDB.Display
            );

            displayModeController = new DisplayModeDropdownController
            (
                root.Q<DropdownField>("DisplayModeDropdown"),
                new DisplayModeSetting(),
                _localizedDB
            );

            vsyncToggleController = new VSyncToggleController
            (
                root.Q<Toggle>("VSyncToggle"),
                new VSyncSetting(),
                _localizedDB
            );

            refreshRateDropdownController = new RefreshRateDropdownController
            (
                root.Q<DropdownField>("RefreshRateDropdown"),
                new RefreshRateSetting(),
                _localizedDB
            );

            frameRateDropdownController = new FrameRateDropdownController
            (
                root.Q<DropdownField>("FrameRateDropdown"),
                new FrameRateLimitSetting(),
                _localizedDB
            );

            optionSettings = new IOptionSetting[5];
            optionSettings[0] = resolutionController.Setting;
            optionSettings[1] = displayModeController.Setting;
            optionSettings[2] = vsyncToggleController.Setting;
            optionSettings[3] = refreshRateDropdownController.Setting;
            optionSettings[4] = frameRateDropdownController.Setting;
        }

        public override void InitializeControllers()
        {
            resolutionController.Initialize();
            displaysController.Initialize();
            displayModeController.Initialize();
            vsyncToggleController.Initialize();
            refreshRateDropdownController.Initialize();
            frameRateDropdownController.Initialize();
        }
       
        public override void RevertSettings()
        {
            resolutionController?.Revert();
            displayModeController?.Revert();
            vsyncToggleController?.Revert();
            refreshRateDropdownController?.Revert();
            frameRateDropdownController?.Revert();
        }

        public override void ApplySettings()
        {
            resolutionController?.Apply();
            displaysController?.Apply();
            displayModeController?.Apply();
            vsyncToggleController?.Apply();
            refreshRateDropdownController?.Apply();
            frameRateDropdownController?.Apply();
        }

        public override bool HasChanges()
        {
            foreach (IOptionSetting setting in optionSettings) 
            {
                if (setting.HasChanged()) return true;
            }
            return false; 
        }

        public void RefreshOnlyUI()
        {
            // 1. Annuler toutes les settings (valeurs internes)
            foreach (var setting in optionSettings)
            {
                setting.Revert();
            }

            // 2. Réinitialiser toutes les UI visuellement
            resolutionController.Initialize();
            displaysController.Initialize();
            displayModeController.Initialize();
            vsyncToggleController.Initialize();
            refreshRateDropdownController.Initialize();
            frameRateDropdownController.Initialize();
        }
    }
}

