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
        private readonly OptionEnum _optionEnum;                                   // Ton widget custom
        private readonly DisplayModeSetting _setting;
        private readonly LocalizedStringDatabase_Options_Display _optionsDB;
        private readonly TMP_Text _label;                                          // Label localisé (optionnel)

        // Ordre d’affichage
        private readonly List<DisplayModeOption> _order = new()
        {
            DisplayModeOption.Fullscreen,
            DisplayModeOption.Borderless,
            DisplayModeOption.MaximizedWindow,
            DisplayModeOption.Windowed
        };

        // Libellés localisés par option (texte final)
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

            // Remplit les choix (remplacement direct pour éviter les doublons)
            var choices = new List<string>(_order.Count);
            foreach (var opt in _order)
                choices.Add(_labels[opt]);

            _optionEnum.Choices = choices;

            // Place l’index courant selon le FullScreenMode actuel
            _optionEnum.CurrentChoiceIndex = IndexFromUnityMode(Screen.fullScreenMode);
            _optionEnum.CheckConstraints(_optionEnum.CurrentChoiceIndex);
            _optionEnum.ForceRebuildUI();

            // Pousse vers le modèle
            UpdateSettingFromIndex(_optionEnum.CurrentChoiceIndex);

            // Écoute les clics utilisateur pour mettre à jour le modèle
            _optionEnum.LeftButton.onClick.AddListener(OnUserChangedChoice);
            _optionEnum.RightButton.onClick.AddListener(OnUserChangedChoice);

            // Label de groupe (si fourni)
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
            _optionEnum.CurrentChoiceIndex = prevIndex;
            _optionEnum.ForceRebuildUI();
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
                _optionEnum.CurrentChoiceIndex = IndexFromUnityMode(Screen.fullScreenMode);
                _optionEnum.ForceRebuildUI();
                UpdateSettingFromIndex(_optionEnum.CurrentChoiceIndex);
            }
        }

        /// <summary>À appeler si tu veux détacher proprement les listeners (ex: OnDestroy du binder).</summary>
        public void Detach()
        {
            if (_optionEnum == null) return;
            _optionEnum.LeftButton.onClick.RemoveListener(OnUserChangedChoice);
            _optionEnum.RightButton.onClick.RemoveListener(OnUserChangedChoice);
        }

        public bool HasChanges() => _setting.HasChanged();

        // ---------- Internes ----------

        private void OnUserChangedChoice()
        {
            UpdateSettingFromIndex(_optionEnum.CurrentChoiceIndex);
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
