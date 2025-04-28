using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    public class RefreshRateDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly RefreshRateSetting setting;
        private LocalizedStringDatabase_Options_Display optionsDB;

        public RefreshRateSetting Setting => setting;

        public RefreshRateDropdownController(DropdownField dropdown, RefreshRateSetting setting, LocalizedStringDatabase_Options_Display optionsDb)
        {
            this.dropdown = dropdown;
            this.setting = setting;
            this.optionsDB = optionsDb;
        }

        public void Initialize()
        {
            if (dropdown == null)
            {
                Debug.LogError("DropdownField is null for RefreshRateDropdownController!");
                return;
            }

            List<string> rates = new();
            foreach (var res in Screen.resolutions)
            {
                string hz = $"{res.refreshRateRatio.value} Hz";
                if (!rates.Contains(hz))
                    rates.Add(hz);
            }

            dropdown.choices = rates;
            string currentHz = $"{Screen.currentResolution.refreshRateRatio.value} Hz";

            dropdown.value = rates.Contains(currentHz) ? currentHz : rates[0];
            UpdateSettingFromValue(dropdown.value);

            dropdown.RegisterValueChangedCallback(evt => UpdateSettingFromValue(evt.newValue));
        }

        private void UpdateSettingFromValue(string value)
        {
            if (uint.TryParse(value.Replace(" Hz", ""), out uint hz))
            {
                setting.SetSelectedRefreshRate(hz);
            }
        }

        public void UpdateLanguage()
        {
            dropdown.label = optionsDB.Refresh_Rate.GetLocalizedString();
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
