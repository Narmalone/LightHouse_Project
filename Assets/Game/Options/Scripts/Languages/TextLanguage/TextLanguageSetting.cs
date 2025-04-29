using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Game.Options
{
    public class TextLanguageSetting : IOptionSetting
    {
        private Locale initialLocale;
        private Locale selectedLocale;

        public TextLanguageSetting()
        {
            initialLocale = LocalizationSettings.SelectedLocale;
            selectedLocale = initialLocale;
        }

        public void SetSelectedLocale(Locale locale)
        {
            selectedLocale = locale;
        }

        public bool HasChanged()
        {
            return selectedLocale != initialLocale;
        }

        public void Apply()
        {
            LocalizationSettings.SelectedLocale = selectedLocale;
            initialLocale = selectedLocale;
        }

        public void Revert()
        {
            selectedLocale = initialLocale;
            LocalizationSettings.SelectedLocale = initialLocale;
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
