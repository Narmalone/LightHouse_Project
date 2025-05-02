using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using LightHouse.Localization;
using LightHouse.UIExtensions;
using UnityEngine.UI;

namespace LightHouse.Game.Options
{
    public struct FrameRateParams
    {
        public string Name;
        public int FrameRateOption;

        public FrameRateParams(string name, int rateOption)
        {
            this.Name = name;
            FrameRateOption = rateOption;
        }
    }

    public class FrameRateDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly FrameRateLimitSetting setting;
        private List<FrameRateParams> frameRateOptions;

        public FrameRateLimitSetting Setting => setting;

        private LocalizedStringDatabase_Options_Display optionsDB;

        public FrameRateDropdownController(DropdownField dropdown, FrameRateLimitSetting setting, LocalizedStringDatabase_Options_Display optionsDisplayDB)
        {
            this.dropdown = dropdown;
            this.setting = setting;
            this.optionsDB = optionsDisplayDB;
        }

        public void InitLocalization()
        {
            frameRateOptions = new List<FrameRateParams>
            {
                new FrameRateParams("30", 30),
                new FrameRateParams("60", 60),
                new FrameRateParams("120", 120),
                new FrameRateParams("240", 240),
                new FrameRateParams(optionsDB.FPS_Unlimited.GetLocalizedString(), -1)
            };
        }

        public void Initialize()
        {
            if (dropdown == null)
            {
                Debug.LogError("DropdownField is null for FrameRateDropdownController!");
                return;
            }

            InitLocalization();
            RefreshDropdown();
            dropdown.RegisterValueChangedCallback(evt => UpdateSettingFromValue(evt.newValue));
        }

        private void RefreshDropdown()
        {
            List<string> fpsChoices = new();
            for(int i = 0; i < frameRateOptions.Count; i++)
            {
                if(i == frameRateOptions.Count - 1)
                {
                    fpsChoices.Add($"{frameRateOptions[i].Name}");
                }
                else
                {
                    fpsChoices.Add($"{frameRateOptions[i].Name} FPS");
                }
            }
            dropdown.UpdateChoices(fpsChoices);

            string currentFPS = Application.targetFrameRate > 0 ? $"{Application.targetFrameRate}" : optionsDB.FPS_Unlimited.GetLocalizedString();
            if (fpsChoices.Contains(currentFPS))
                dropdown.SetValueWithoutNotify(currentFPS);

            dropdown.value = currentFPS;
            UpdateSettingFromValue(dropdown.value);
        }

        private void UpdateSettingFromValue(string value)
        {
            if (int.TryParse(value.Replace(" FPS", ""), out int fps))
            {
                setting.SetSelectedFrameRate(fps);
            }
            else
            {
                setting.SetSelectedFrameRate(-1);
            }
        }

        public void UpdateLanguage()
        {
            dropdown.label = optionsDB.FPS_Limit.GetLocalizedString();

            // Update localized text for Unlimited option
            frameRateOptions[frameRateOptions.Count - 1] = new FrameRateParams(optionsDB.FPS_Unlimited.GetLocalizedString(), -1);

            RefreshDropdown(); // 👈 Force update full choices with localized names
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
