using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    public class ResolutionDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly ResolutionSetting setting;
        private LocalizedStringDatabase_Options_Display optionDB;

        public ResolutionSetting Setting => setting;

        public ResolutionDropdownController(DropdownField dropdown, ResolutionSetting setting, LocalizedStringDatabase_Options_Display optionDisplayDB)
        {
            this.dropdown = dropdown;
            this.setting = setting;
            this.optionDB = optionDisplayDB;
        }

        public void Initialize()
        {
            if (dropdown == null)
            {
                Debug.LogError("DropdownField is null for ResolutionDropdownController!");
                return;
            }

            List<string> resolutions = new();
            int maxWidth = Display.main.systemWidth;
            int maxHeight = Display.main.systemHeight;

            foreach (var res in Screen.resolutions)
            {
                if (res.width <= maxWidth && res.height <= maxHeight)
                {
                    resolutions.Add($"{res.width}x{res.height}");
                }
            }

            if (resolutions.Count == 0)
                resolutions.Add($"{maxWidth}x{maxHeight}");

            dropdown.choices = resolutions;
            string current = $"{Screen.width}x{Screen.height}";
            dropdown.value = resolutions.Contains(current) ? current : resolutions[resolutions.Count - 1];
            UpdateSettingFromValue(dropdown.value);

            dropdown.RegisterValueChangedCallback(evt => UpdateSettingFromValue(evt.newValue));
        }


        public void UpdateLanguage()
        {
            dropdown.label = optionDB.Resolution.GetLocalizedString();
        }

        private void UpdateSettingFromValue(string value)
        {
            var parts = value.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            {
                Vector2Int parsedResolution = new Vector2Int(w, h);
                setting.SetSelectedResolution(parsedResolution);
            }
        }


        public void Apply()
        {
            if (setting.HasChanged()) setting.Apply();
        }

        public void Revert()
        {
            if (setting.HasChanged())
            {
                setting.Revert();
                Initialize();
            }
        }
    }
}
