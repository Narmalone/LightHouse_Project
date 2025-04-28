using System;
using System.Collections;
using System.Collections.Generic;
using LightHouse.Localization;
using LightHouse.UIExtensions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    public class TextLanguagesDropdownController
    {
        private readonly DropdownField dropdown;
        private LocalizedString _dropdownName;

        public TextLanguagesDropdownController(DropdownField dropdown, LocalizedString dropdownName)
        {
            this.dropdown = dropdown;
            _dropdownName = dropdownName;
        }

        public void Initialize()
        {
            dropdown.RegisterValueChangedCallback(OnDropDownChanged);
            InitLocalization();
        }

        private void OnDropDownChanged(ChangeEvent<string> evt)
        {
            int selectedIndex = dropdown.index;
            if (selectedIndex >= 0 && selectedIndex < LocalizationSettings.AvailableLocales.Locales.Count)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[selectedIndex];
                //Debug.Log($"Selected new locale: {LocalizationSettings.SelectedLocale.Identifier.Code}");
            }
        }

        public void InitLocalization()
        {
            List<string> localesNames = new List<string>();

            foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
            {
                string nativeName = locale.Identifier.CultureInfo.NativeName;
                localesNames.Add(CapitalizeFirstLetter(nativeName));
            }

            dropdown.UpdateChoices(localesNames);
            dropdown.index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        }

        public void UpdateLanguage()
        {
            dropdown.label = _dropdownName.GetLocalizedString();
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.Length == 1)
                return input.ToUpper();

            return char.ToUpper(input[0]) + input.Substring(1);
        }

    }
}
