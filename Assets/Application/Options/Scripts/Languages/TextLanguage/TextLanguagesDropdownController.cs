using LightHouse.Localization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// Contrôleur du menu déroulant de langues (UGUI/TMP).
    /// - Peuple le TMP_Dropdown avec les locales disponibles
    /// - Met à jour la locale sélectionnée via LocalizationSettings
    /// - Affiche un label localisé au-dessus (TMP_Text séparé)
    /// </summary>
    [Serializable]
    public class TextLanguagesDropdownController
    {
        private TextLanguageSetting _textLanguageSetting;

        [SerializeField] private TMP_Dropdown _dropdown;   // Canvas/TMP
        [SerializeField] private TMP_Text _labelText;      // Label affiché à côté/au-dessus du dropdown
        [SerializeField] private LocalizedString _dropdownName; // Clé LocalizedString pour le label

        public TextLanguageSetting Setting => _textLanguageSetting;

        public TextLanguagesDropdownController(TMP_Dropdown dropdown, LocalizedString dropdownName)
        {
            _dropdown = dropdown;
            _dropdownName = dropdownName;
            _textLanguageSetting = new TextLanguageSetting();
        }

        /// <summary>
        /// Surchargée si tu veux binder le label via code plutôt que par l’inspecteur.
        /// </summary>
        public TextLanguagesDropdownController(TMP_Dropdown dropdown, TMP_Text labelText, LocalizedString dropdownName)
            : this(dropdown, dropdownName)
        {
            _labelText = labelText;
        }

        public void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError("[TextLanguagesDropdownController] TMP_Dropdown manquant.");
                return;
            }

            // Nettoie les anciens listeners pour éviter les doublons
            _dropdown.onValueChanged.RemoveListener(OnDropDownChanged);
            _dropdown.onValueChanged.AddListener(OnDropDownChanged);

            // Met en place la localisation (label + options)
            InitLocalization();

            // Abonnement pour mettre à jour le label quand la locale change
            _dropdownName.StringChanged -= OnLabelStringChanged;
            _dropdownName.StringChanged += OnLabelStringChanged;
            UpdateLanguage(); // première maj du label
        }

        public void Dispose()
        {
            if (_dropdown != null)
                _dropdown.onValueChanged.RemoveListener(OnDropDownChanged);

            _dropdownName.StringChanged -= OnLabelStringChanged;
        }

        private void OnDropDownChanged(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < LocalizationSettings.AvailableLocales.Locales.Count)
            {
                var selectedLocale = LocalizationSettings.AvailableLocales.Locales[selectedIndex];
                _textLanguageSetting.SetSelectedLocale(selectedLocale); // applique et persiste (selon ta classe)
            }
        }

        public void InitLocalization()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;

            // 1) Peuple les options du dropdown
            _dropdown.options.Clear();

            List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>(locales.Count);
            foreach (var locale in locales)
            {
                // Nom natif avec maj. initiale (comme tu le faisais)
                string nativeName = locale.Identifier.CultureInfo.NativeName;
                optionData.Add(new TMP_Dropdown.OptionData(CapitalizeFirstLetter(nativeName)));
            }

            _dropdown.options = optionData;

            // 2) Sélection initiale = locale actuelle
            int currentIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
            _dropdown.value = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, locales.Count - 1));
            _dropdown.RefreshShownValue();
        }

        public void UpdateLanguage()
        {
            // Met à jour le label localisé du champ (pas le caption du dropdown)
            if (_labelText != null)
            {
                _labelText.text = _dropdownName.GetLocalizedString();
            }
        }

        private void OnLabelStringChanged(string newValue)
        {
            if (_labelText != null)
                _labelText.text = newValue;
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length == 1) return input.ToUpper();
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// Restaure l’ancienne locale si l’utilisateur annule.
        /// </summary>
        public void Revert()
        {
            if (_textLanguageSetting != null && _textLanguageSetting.HasChanged())
            {
                _textLanguageSetting.Revert();
                InitLocalization(); // remet l’UI en phase avec la locale restaurée
            }
        }
    }
}
