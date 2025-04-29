using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq; // Ajout important pour le tri
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

            int maxWidth = Display.main.systemWidth;
            int maxHeight = Display.main.systemHeight;

            HashSet<double> rates = new();

            foreach (var res in Screen.resolutions)
            {
                if (res.width <= maxWidth && res.height <= maxHeight)
                {
                    rates.Add(res.refreshRateRatio.value); // 👈 NE PAS CASTER EN UINT
                }
            }

            // Ajouter la fréquence actuelle si absente
            double currentRefreshRate = Screen.currentResolution.refreshRateRatio.value;
            if (!rates.Any(rate => Mathf.Abs((float)(rate - currentRefreshRate)) < 0.01f))
            {
                Debug.Log($"[RefreshRateDropdown] Adding current refresh rate: {currentRefreshRate:F3} Hz");
                rates.Add(currentRefreshRate);
            }

            List<string> sortedRates = rates
                .OrderBy(r => r)
                .Select(r => $"{r:F2} Hz") // 👈 Formater proprement 59.94 etc.
                .ToList();

            if (sortedRates.Count == 0)
            {
                sortedRates.Add($"{currentRefreshRate:F2} Hz");
            }

            dropdown.choices = sortedRates;

            string currentHz = $"{currentRefreshRate:F2} Hz";

            dropdown.value = sortedRates.Contains(currentHz) ? currentHz : sortedRates[sortedRates.Count - 1];
            UpdateSettingFromValue(dropdown.value);

            dropdown.RegisterValueChangedCallback(evt => UpdateSettingFromValue(evt.newValue));
        }

        private void UpdateSettingFromValue(string value)
        {
            if (value != null && value.Contains("Hz"))
            {
                if (float.TryParse(value.Replace(" Hz", ""), out float hz))
                {
                    setting.SetSelectedRefreshRate(hz);
                }
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
