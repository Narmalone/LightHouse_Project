using UnityEngine;
using System.Collections.Generic;
using TMPro;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// Contrôleur pour un TMP_Dropdown en Canvas (uGUI) listant les résolutions uniques (WxH).
    /// </summary>
    public class ResolutionDropdownController : IOptionController
    {
        private readonly TMP_Dropdown _dropdown;                    // uGUI TMP_Dropdown
        private readonly ResolutionSetting _setting;                // Ton modèle de donnée
        private readonly LocalizedStringDatabase_Options_Display _optionDB;
        private readonly TMP_Text _label;                           // Label séparé pour la localisation (facultatif)

        // Cache des résolutions (unique par largeur/hauteur) alignées sur les options du dropdown
        private readonly List<Vector2Int> _uniqueResolutions = new();

        public ResolutionSetting Setting => _setting;

        public ResolutionDropdownController(
            TMP_Dropdown dropdown,
            ResolutionSetting setting,
            LocalizedStringDatabase_Options_Display optionDisplayDB,
            TMP_Text labelForLocalization = null)
        {
            _dropdown = dropdown;
            _setting = setting;
            _optionDB = optionDisplayDB;
            _label = labelForLocalization;
        }

        /// <summary>
        /// Initialise la liste et connecte les callbacks.
        /// </summary>
        public void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError("[ResolutionDropdownController] TMP_Dropdown est null.");
                return;
            }

            // Sécurités affichage — borne aux dimensions de l’écran principal
            int maxWidth = Display.main?.systemWidth ?? Screen.currentResolution.width;
            int maxHeight = Display.main?.systemHeight ?? Screen.currentResolution.height;

            _uniqueResolutions.Clear();

            // 1) Récupère toutes les résolutions disponibles
            // 2) Filtre par taille max de l’écran
            // 3) Dé-duplique sur largeur/hauteur (ignorer le refresh rate)
            // 4) Trie par (width asc, height asc)
            var set = new HashSet<string>();
            foreach (var r in Screen.resolutions)
            {
                if (r.width > maxWidth || r.height > maxHeight) continue;

                string key = $"{r.width}x{r.height}";
                if (set.Add(key)) _uniqueResolutions.Add(new Vector2Int(r.width, r.height));
            }

            if (_uniqueResolutions.Count == 0)
            {
                // Fallback: au moins la résolution actuelle ou la borne max
                var fallback = new Vector2Int(Mathf.Max(Screen.width, maxWidth),
                                              Mathf.Max(Screen.height, maxHeight));
                _uniqueResolutions.Add(fallback);
            }

            _uniqueResolutions.Sort((a, b) =>
            {
                int c = a.x.CompareTo(b.x);
                return c != 0 ? c : a.y.CompareTo(b.y);
            });

            // Remplit les options du TMP_Dropdown
            var options = new List<TMP_Dropdown.OptionData>(_uniqueResolutions.Count);
            foreach (var v in _uniqueResolutions)
                options.Add(new TMP_Dropdown.OptionData($"{v.x}x{v.y}"));

            _dropdown.options = options;

            // Sélectionne l'index correspondant à la résolution actuelle (ou la plus grande dispo)
            var current = new Vector2Int(Screen.width, Screen.height);
            int index = IndexOfResolutionOrNearest(current);

            // Débranche/branche proprement pour éviter double invocation
            _dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            _dropdown.value = Mathf.Clamp(index, 0, _dropdown.options.Count - 1);
            _dropdown.RefreshShownValue();
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

            // Pousse la valeur vers le modèle
            UpdateSettingFromIndex(_dropdown.value);
        }

        /// <summary>
        /// Met à jour le label localisé (si fourni).
        /// </summary>
        public void UpdateLanguage()
        {
            if (_label != null && _optionDB != null)
                _label.text = _optionDB.Resolution.GetLocalizedString();
        }

        /// <summary>
        /// Applique la résolution via le modèle.
        /// </summary>
        public void Apply()
        {
            if (_setting.HasChanged()) _setting.Apply();
        }

        /// <summary>
        /// Revient à la valeur précédente et réinitialise l’affichage.
        /// </summary>
        public void Revert()
        {
            if (_setting.HasChanged())
            {
                _setting.Revert();
                Initialize(); // Re-sync dropdown avec l'état réel
            }
        }

        // --- Internals ---

        private void OnDropdownValueChanged(int newIndex)
        {
            UpdateSettingFromIndex(newIndex);
        }

        private void UpdateSettingFromIndex(int index)
        {
            if (index < 0 || index >= _uniqueResolutions.Count) return;
            var res = _uniqueResolutions[index];
            _setting.SetSelectedResolution(res);
        }

        private int IndexOfResolutionOrNearest(Vector2Int target)
        {
            // Try exact
            int exact = _uniqueResolutions.FindIndex(r => r.x == target.x && r.y == target.y);
            if (exact >= 0) return exact;

            // Sinon, prend la plus grande <= target; sinon la plus grande
            int idx = -1;
            for (int i = 0; i < _uniqueResolutions.Count; i++)
            {
                var r = _uniqueResolutions[i];
                if (r.x <= target.x && r.y <= target.y) idx = i;
            }
            return idx >= 0 ? idx : _uniqueResolutions.Count - 1;
        }
    }
}
