using UnityEngine.UIElements;
using LightHouse.Localization;

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

        protected IOptionSetting[] optionsSettings;
        #endregion

        public DisplayOptionsWindow(VisualElement root, ConfirmationPopupController confirmationPopup, LocalizedStringDatabase_Options_Display optionDB) : base(root, confirmationPopup)
        {
            _localizedDB = optionDB;
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

        public override void InitializeControllers()
        {
            resolutionController = new ResolutionDropdownController
            (
                root.Q<DropdownField>("ResolutionDropdown"),
                new ResolutionSetting(),
                optionDisplayDB: _localizedDB
            );
            resolutionController.Initialize();

            displaysController = new DisplaysDropdownController
            (
                root.Q<DropdownField>("DisplaysDropdown"),
                new DisplaysSetting(),
                _localizedDB
            );
            displaysController.Initialize();

            displayModeController = new DisplayModeDropdownController
            (
                root.Q<DropdownField>("DisplayModeDropdown"),
                new DisplayModeSetting(),
                _localizedDB
            );
            displayModeController.Initialize();

            vsyncToggleController = new VSyncToggleController
            (
                root.Q<Toggle>("VSyncToggle"),
                new VSyncSetting(),
                _localizedDB
            );
            vsyncToggleController.Initialize();

            refreshRateDropdownController = new RefreshRateDropdownController
            (
                root.Q<DropdownField>("RefreshRateDropdown"),
                new RefreshRateSetting(),
                _localizedDB
            );
            refreshRateDropdownController.Initialize();

            frameRateDropdownController = new FrameRateDropdownController
            (
                root.Q<DropdownField>("FrameRateDropdown"),
                new FrameRateLimitSetting(),
                _localizedDB
            );
            frameRateDropdownController.Initialize();

            optionsSettings = new IOptionSetting[6];
            optionsSettings[0] = resolutionController.Setting;
            optionsSettings[1] = displaysController.Setting;
            optionsSettings[2] = displayModeController.Setting;
            optionsSettings[3] = vsyncToggleController.Setting;
            optionsSettings[4] = refreshRateDropdownController.Setting;
            optionsSettings[5] = frameRateDropdownController.Setting;
        }

        public override void RevertSettings()
        {
            resolutionController?.Revert();
            displaysController?.Revert();
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
            foreach (IOptionSetting setting in optionsSettings) 
            {
                if (setting.HasChanged()) return true;
            }
            return false; 
        }
    }
}

