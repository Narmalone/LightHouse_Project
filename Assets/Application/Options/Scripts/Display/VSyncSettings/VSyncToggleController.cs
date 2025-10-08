using LightHouse.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// Contrôleur pour le toggle VSync (uGUI/Canvas version).
    /// </summary>
    public class VSyncToggleController : IOptionController
    {
        private readonly OptionToggle _toggle;                                         // Toggle uGUI
        private readonly VSyncSetting _setting;                                  // Données
        private readonly LocalizedStringDatabase_Options_Display _optionsDB;     // DB de textes localisés
        private readonly TMP_Text _label;                                        // Label optionnel pour la localisation

        public VSyncSetting Setting => _setting;

        public VSyncToggleController(
            OptionToggle toggle,
            VSyncSetting setting,
            LocalizedStringDatabase_Options_Display optionsDB,
            TMP_Text localizedLabel = null)
        {
            _toggle = toggle;
            _setting = setting;
            _optionsDB = optionsDB;
            _label = localizedLabel;
        }

        /// <summary>
        /// Initialise le toggle selon l’état actuel du VSync et lie les callbacks.
        /// </summary>
        public void Initialize()
        {
            if (_toggle == null)
            {
                Debug.LogError("[VSyncToggleController] Toggle is null!");
                return;
            }

            bool isVSyncOn = QualitySettings.vSyncCount > 0;

            _toggle.OnValueChanged += OnToggleChanged;
            _toggle.isOn = isVSyncOn;
            _toggle.OnValueChanged += OnToggleChanged;

            _setting.SetSelectedVSync(isVSyncOn);

            if (_label != null)
                _label.text = _optionsDB.VSync.GetLocalizedString();
        }

        /// <summary>
        /// Met à jour la langue du label (si présent).
        /// </summary>
        public void UpdateLanguage()
        {
            if (_label != null)
                _label.text = _optionsDB.VSync.GetLocalizedString();
        }

        /// <summary>
        /// Applique la valeur au système.
        /// </summary>
        public void Apply()
        {
            if (_setting.HasChanged())
                _setting.Apply();
        }

        /// <summary>
        /// Rétablit la valeur précédente.
        /// </summary>
        public void Revert()
        {
            if (_setting.HasChanged())
            {
                _setting.Revert();
                bool currentVSync = QualitySettings.vSyncCount > 0;
                _toggle.SetValueWithoutNotify(currentVSync);
            }
        }

        // --- Internes ---

        private void OnToggleChanged(bool newValue)
        {
            _setting.SetSelectedVSync(newValue);
        }
    }
}
