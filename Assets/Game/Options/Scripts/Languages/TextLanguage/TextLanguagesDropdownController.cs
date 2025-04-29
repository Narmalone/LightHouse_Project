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
        private TextLanguageSetting _textLanguageSetting;
        private readonly DropdownField dropdown;
        private LocalizedString _dropdownName;

        public TextLanguageSetting Setting => _textLanguageSetting;
        public TextLanguagesDropdownController(DropdownField dropdown, LocalizedString dropdownName)
        {
            this.dropdown = dropdown;
            _dropdownName = dropdownName;
            _textLanguageSetting = new TextLanguageSetting();
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
                var selectedLocale = LocalizationSettings.AvailableLocales.Locales[selectedIndex];
                _textLanguageSetting.SetSelectedLocale(selectedLocale); // <--- important !
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
