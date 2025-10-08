using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// Contrôleur du mode d’affichage basé sur le widget custom OptionEnum (gauche/droite).
    /// </summary>
    public class DisplayModeOptionEnumController
    {
        private readonly OptionEnum _optionEnum;
        private readonly DisplayModeSetting _setting;
        private readonly LocalizedStringDatabase_Options_Display _optionsDB;
        private readonly TMP_Text _label;

        // Ordre d’affichage
        private readonly List<DisplayModeOption> _order = new()
        {
            DisplayModeOption.Fullscreen,
            DisplayModeOption.Borderless,
            DisplayModeOption.MaximizedWindow,
            DisplayModeOption.Windowed
        };

        // Libellés localisés par option
        private readonly Dictionary<DisplayModeOption, string> _labels = new();

        public DisplayModeSetting Setting => _setting;

        public DisplayModeOptionEnumController(
            OptionEnum optionEnum,
            DisplayModeSetting setting,
            LocalizedStringDatabase_Options_Display optionsDB,
            TMP_Text localizedLabel = null)
        {
            _optionEnum = optionEnum;
            _setting = setting;
            _optionsDB = optionsDB;
            _label = localizedLabel;
        }

        /// <summary>Initialise la liste, sélectionne l’état courant, abonne les callbacks.</summary>
        public void Initialize()
        {
            if (_optionEnum == null)
            {
                Debug.LogError("[DisplayModeOptionEnumController] OptionEnum est null.");
                return;
            }

            BuildLocalizedLabels();

            // Remplit les choix localisés
            var choices = new List<string>(_order.Count);
            foreach (var opt in _order)
                choices.Add(_labels[opt]);

            _optionEnum.Choices = choices;

            // Sélectionne l’état courant
            int currentIndex = IndexFromUnityMode(Screen.fullScreenMode);
            _optionEnum.SetSelectedIndex(currentIndex);

            // Pousse vers le modèle
            UpdateSettingFromIndex(currentIndex);

            // Écoute l’événement (plus simple que les clics de boutons)
            _optionEnum.OnValueChanged -= OnOptionChanged;
            _optionEnum.OnValueChanged += OnOptionChanged;

            // Label localisé
            if (_label != null)
                _label.text = _optionsDB.Display_Mode.GetLocalizedString();
        }

        /// <summary>À appeler quand la langue change.</summary>
        public void UpdateLanguage()
        {
            if (_label != null)
                _label.text = _optionsDB.Display_Mode.GetLocalizedString();

            int prevIndex = Mathf.Clamp(_optionEnum.CurrentChoiceIndex, 0, _order.Count - 1);

            BuildLocalizedLabels();

            var choices = new List<string>(_order.Count);
            foreach (var opt in _order)
                choices.Add(_labels[opt]);

            _optionEnum.Choices = choices;
            _optionEnum.SetSelectedIndex(prevIndex);
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
                int idx = IndexFromUnityMode(Screen.fullScreenMode);
                _optionEnum.SetSelectedIndex(idx);
                UpdateSettingFromIndex(idx);
            }
        }

        /// <summary>Détache les callbacks (utile dans OnDestroy).</summary>
        public void Detach()
        {
            if (_optionEnum == null) return;
            _optionEnum.OnValueChanged -= OnOptionChanged;
        }

        public bool HasChanges() => _setting.HasChanged();

        // ---------- Internes ----------

        private void OnOptionChanged(int newIndex)
        {
            UpdateSettingFromIndex(newIndex);
        }

        private void UpdateSettingFromIndex(int index)
        {
            index = Mathf.Clamp(index, 0, _order.Count - 1);
            var mode = _order[index];

            var unityMode = mode switch
            {
                DisplayModeOption.Fullscreen => FullScreenMode.ExclusiveFullScreen,
                DisplayModeOption.Borderless => FullScreenMode.FullScreenWindow,
                DisplayModeOption.MaximizedWindow => FullScreenMode.MaximizedWindow,
                DisplayModeOption.Windowed => FullScreenMode.Windowed,
                _ => FullScreenMode.FullScreenWindow
            };

            _setting.SetSelectedMode(unityMode);
        }

        private int IndexFromUnityMode(FullScreenMode unityMode)
        {
            var mode = unityMode switch
            {
                FullScreenMode.ExclusiveFullScreen => DisplayModeOption.Fullscreen,
                FullScreenMode.FullScreenWindow => DisplayModeOption.Borderless,
                FullScreenMode.MaximizedWindow => DisplayModeOption.MaximizedWindow,
                FullScreenMode.Windowed => DisplayModeOption.Windowed,
                _ => DisplayModeOption.Fullscreen
            };
            return _order.IndexOf(mode);
        }

        private void BuildLocalizedLabels()
        {
            _labels[DisplayModeOption.Fullscreen] = _optionsDB.DisplayMode_Fullscreen.GetLocalizedString();
            _labels[DisplayModeOption.Borderless] = _optionsDB.DisplayMode_Borderless.GetLocalizedString();
            _labels[DisplayModeOption.MaximizedWindow] = _optionsDB.DisplayMode_MaximizedWindow.GetLocalizedString();
            _labels[DisplayModeOption.Windowed] = _optionsDB.DisplayMode_Windowed.GetLocalizedString();
        }
    }
}
