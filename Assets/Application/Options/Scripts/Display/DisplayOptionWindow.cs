using LightHouse.Localization;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LightHouse.Game.Options
{
    public class DisplayOptionsWindow : OptionWindowBase
    {
        #region FIELDS

        [SerializeField] protected LocalizedStringDatabase_Options_Display _localizedDB;

        [Header("Resolution")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        [Header("Display Mode")]
        [SerializeField] private OptionEnum _displayModeDropdown;

        [Header("Monitors")]
        [SerializeField] private OptionEnum _monitorsDropdown;

        [Header("Monitors")]
        [SerializeField] private TMP_Dropdown _refreshRateDropdown;

        [Header("Monitors")]
        [SerializeField] private TMP_Dropdown _frameRateDropdown;

        [Header("Monitors")]
        [SerializeField] private OptionToggle _vSyncToggle;

        private ResolutionDropdownController resolutionController;
        private MonitorsEnumController displaysController;
        private DisplayModeOptionEnumController displayModeController;
        private VSyncToggleController vsyncToggleController;
        private RefreshRateDropdownController refreshRateDropdownController;
        private FrameRateDropdownController frameRateDropdownController;
        #endregion

        private void Start()
        {
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
                _resolutionDropdown,
                new ResolutionSetting(),
                optionDisplayDB: _localizedDB
            );

            displaysController = new MonitorsEnumController
            (
                _monitorsDropdown,
                confirmationPopupController,
                _localizedDB.Display
            );

            displayModeController = new DisplayModeOptionEnumController
            (
                _displayModeDropdown,
                new DisplayModeSetting(),
                _localizedDB
            );

            vsyncToggleController = new VSyncToggleController
            (
                _vSyncToggle,
                new VSyncSetting(),
                _localizedDB
            );

            refreshRateDropdownController = new RefreshRateDropdownController
            (
                _refreshRateDropdown,
                new RefreshRateSetting(),
                _localizedDB
            );

            frameRateDropdownController = new FrameRateDropdownController
            (
                _frameRateDropdown,
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
            foreach (var setting in optionSettings)
            {
                setting.Revert();
            }

            resolutionController.Initialize();
            displaysController.Initialize();
            displayModeController.Initialize();
            vsyncToggleController.Initialize();
            refreshRateDropdownController.Initialize();
            frameRateDropdownController.Initialize();
        }
    }
}

