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

            HashSet<uint> rates = new();

            foreach (var res in Screen.resolutions)
            {
                if (res.width <= maxWidth && res.height <= maxHeight)
                {
                    rates.Add((uint)res.refreshRateRatio.value);
                }
            }

            // Force ajouter la fréquence actuelle si elle n'existe pas
            uint currentRefreshRate = (uint)Screen.currentResolution.refreshRateRatio.value;
            if (!rates.Contains(currentRefreshRate))
            {
                Debug.Log($"[RefreshRateDropdown] Adding current refresh rate: {currentRefreshRate} Hz");
                rates.Add(currentRefreshRate);
            }

            List<string> sortedRates = rates.OrderBy(r => r).Select(r => $"{r} Hz").ToList();

            if (sortedRates.Count == 0)
            {
                sortedRates.Add($"{currentRefreshRate} Hz");
            }

            dropdown.choices = sortedRates;

            string currentHz = $"{currentRefreshRate} Hz";

            dropdown.value = sortedRates.Contains(currentHz) ? currentHz : sortedRates[sortedRates.Count - 1];
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
