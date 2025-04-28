using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    public class DisplaysDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly DisplaysSetting setting;
        private LocalizedStringDatabase_Options_Display optionDB;

        public DisplaysSetting Setting => setting;

        public DisplaysDropdownController(DropdownField dropdown, DisplaysSetting setting, LocalizedStringDatabase_Options_Display optionDisplayDB)
        {
            this.dropdown = dropdown;
            this.setting = setting;
            this.optionDB = optionDisplayDB;
        }

        public void Initialize()
        {
            if (dropdown == null)
            {
                Debug.LogError("DropdownField is null for DisplayDropdownController!");
                return;
            }

            List<string> displays = new();
            for (int i = 0; i < Display.displays.Length; i++)
            {
                displays.Add($"Display {i + 1} ({Display.displays[i].systemWidth}x{Display.displays[i].systemHeight})");
                Debug.Log($"Display {i + 1} ({Display.displays[i].systemWidth}x{Display.displays[i].systemHeight})");
            }

            if (displays.Count == 0)
            {
                displays.Add("Display 1 (Default)");
            }

            dropdown.choices = displays;
            dropdown.value = displays[setting.SelectedDisplay];
            dropdown.RegisterValueChangedCallback(evt => UpdateSettingFromValue(evt.newValue));
        }

        public void UpdateLanguage()
        {
            //dropdown.label = optionDB.Display.GetLocalizedString();
        }

        private void UpdateSettingFromValue(string value)
        {
            int index = dropdown.choices.IndexOf(value);
            if (index >= 0)
            {
                setting.SetSelectedDisplay(index);
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
            }
        }
    }
}
