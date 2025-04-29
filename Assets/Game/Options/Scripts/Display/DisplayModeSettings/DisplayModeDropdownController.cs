using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using System.Collections.Generic;
using LightHouse.Localization;
using LightHouse.UIExtensions; 

namespace LightHouse.Game.Options
{
    public class DisplayModeDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly DisplayModeSetting setting;
        private Dictionary<DisplayModeOption, LocalizedString> displayModeLabels;
        private readonly LocalizedStringDatabase_Options_Display optionsDisplayDB;

        public DisplayModeSetting Setting => setting;
        private DisplayModeOption selectedMode;

        public DisplayModeDropdownController(DropdownField dropdown, DisplayModeSetting setting, LocalizedStringDatabase_Options_Display optionsDB)
        {
            this.dropdown = dropdown;
            this.setting = setting;
            this.optionsDisplayDB = optionsDB;
        }

        public void InitLocalization()
        {
            displayModeLabels = new Dictionary<DisplayModeOption, LocalizedString>
            {
                { DisplayModeOption.Fullscreen, optionsDisplayDB.DisplayMode_Fullscreen },
                { DisplayModeOption.Borderless, optionsDisplayDB.DisplayMode_Borderless },
                { DisplayModeOption.MaximizedWindow, optionsDisplayDB.DisplayMode_MaximizedWindow },
                { DisplayModeOption.Windowed, optionsDisplayDB.DisplayMode_Windowed }
            };
        }

        public void Initialize()
        {
            if (dropdown == null)
            {
                Debug.LogError("DropdownField is null for DisplayModeDropdownController!");
                return;
            }
            InitLocalization();
            UpdateChoices();
            UpdateCurrentMode();
            dropdown.RegisterValueChangedCallback(OnModeChanged);
        }

        public void UpdateChoices()
        {
            if (dropdown == null)
                return;

            List<string> newChoices = new();
            foreach (var label in displayModeLabels.Values)
                newChoices.Add(label.GetLocalizedString());

            dropdown.UpdateChoices(newChoices); // ✅ Utilise l'extension propre ici
        }

        public void UpdateLanguage()
        {
            dropdown.label = optionsDisplayDB.Display_Mode.GetLocalizedString();
            UpdateChoices();
        }

        private void UpdateCurrentMode()
        {
            FullScreenMode unityMode = Screen.fullScreenMode;
            switch (unityMode)
            {
                case FullScreenMode.ExclusiveFullScreen:
                    selectedMode = DisplayModeOption.Fullscreen;
                    break;
                case FullScreenMode.FullScreenWindow:
                    selectedMode = DisplayModeOption.Borderless;
                    break;
                case FullScreenMode.MaximizedWindow:
                    selectedMode = DisplayModeOption.MaximizedWindow;
                    break;
                case FullScreenMode.Windowed:
                    selectedMode = DisplayModeOption.Windowed;
                    break;
            }

            dropdown.SetValueWithoutNotify(displayModeLabels[selectedMode].GetLocalizedString());
            setting.SetSelectedMode(unityMode);
        }

        private void OnModeChanged(ChangeEvent<string> evt)
        {
            foreach (var pair in displayModeLabels)
            {
                if (pair.Value.GetLocalizedString() == evt.newValue)
                {
                    selectedMode = pair.Key;
                    FullScreenMode unityMode = selectedMode switch
                    {
                        DisplayModeOption.Fullscreen => FullScreenMode.ExclusiveFullScreen,
                        DisplayModeOption.Windowed => FullScreenMode.Windowed,
                        DisplayModeOption.Borderless => FullScreenMode.FullScreenWindow,
                        _ => FullScreenMode.ExclusiveFullScreen
                    };

                    setting.SetSelectedMode(unityMode);
                    break;
                }
            }
        }

        public void Apply()
        {
            if (setting.HasChanged())
            {
                setting.Apply();
            }
        }

        public void Revert()
        {
            if (setting.HasChanged())
            {
                setting.Revert();
                UpdateCurrentMode();
            }
        }

        public bool HasChanges()
        {
            return setting.HasChanged();
        }
    }
}
