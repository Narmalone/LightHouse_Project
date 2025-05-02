using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    public class RefreshRateDropdownController
    {
        private readonly DropdownField _dropdown;
        private readonly RefreshRateSetting _setting;
        private LocalizedStringDatabase_Options_Display _optionTextsDB;

        private List<float> _uniqueRates = new();

        public RefreshRateSetting Setting => _setting;

        public RefreshRateDropdownController(DropdownField dropdown, RefreshRateSetting setting, LocalizedStringDatabase_Options_Display optionsDb)
        {
            this._dropdown = dropdown;
            this._setting = setting;
            this._optionTextsDB = optionsDb;
        }

        public void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError("[RefreshRateDropdown] DropdownField is null!");
                return;
            }

            _uniqueRates.Clear();

            int maxWidth = Display.main.systemWidth;
            int maxHeight = Display.main.systemHeight;

            foreach (var res in Screen.resolutions)
            {
                if (res.width <= maxWidth && res.height <= maxHeight)
                {
                    float rate = (float)res.refreshRateRatio.value;
                    if (!_uniqueRates.Any(existing => Mathf.Abs(existing - rate) < 0.01f))
                    {
                        _uniqueRates.Add(rate);
                    }
                }
            }

            float currentRate = (float)Screen.currentResolution.refreshRateRatio.value;
            if (!_uniqueRates.Any(r => Mathf.Abs(r - currentRate) < 0.01f))
            {
                Debug.Log($"[RefreshRateDropdown] Adding current refresh rate: {currentRate:F2} Hz");
                _uniqueRates.Add(currentRate);
            }

            _uniqueRates = _uniqueRates.OrderBy(r => r).ToList();

            List<string> rateStrings = _uniqueRates.Select(r => $"{r:F2} Hz").ToList();
            _dropdown.choices = rateStrings;

            string currentHz = $"{(float)Screen.currentResolution.refreshRateRatio.value:F2} Hz";
            _dropdown.SetValueWithoutNotify(rateStrings.Contains(currentHz) ? currentHz : rateStrings.Last());

            UpdateSettingFromValue(_dropdown.value);
            _dropdown.RegisterValueChangedCallback(evt => UpdateSettingFromValue(evt.newValue));
        }

        private void UpdateSettingFromValue(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.EndsWith("Hz"))
            {
                if (float.TryParse(value.Replace(" Hz", ""), out float hz))
                {
                    _setting.SetSelectedRefreshRate(hz);
                }
            }
        }

        public void UpdateLanguage()
        {
            _dropdown.label = _optionTextsDB.Refresh_Rate.GetLocalizedString();
        }

        public void Apply()
        {
            if (_setting.HasChanged())
                _setting.Apply();
        }

        public void Revert()
        {
            if (_setting.HasChanged())
            {
                _setting.Revert();
                Initialize();
            }
        }
    }
}
