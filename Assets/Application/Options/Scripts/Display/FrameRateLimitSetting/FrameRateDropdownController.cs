using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    public struct FrameRateParams
    {
        public string Name;   // Texte affiché (ex: "60 FPS" ou "Unlimited")
        public int Rate;      // -1 = illimité, sinon FPS cible

        public FrameRateParams(string name, int rate)
        {
            Name = name;
            Rate = rate;
        }
    }

    /// <summary>
    /// Contrôleur d’un TMP_Dropdown (Canvas) pour limiter le framerate: 30/60/120/240/Illimité.
    /// </summary>
    public class FrameRateDropdownController : IOptionController
    {
        private readonly TMP_Dropdown _dropdown;
        private readonly FrameRateLimitSetting _setting;
        private readonly LocalizedStringDatabase_Options_Display _optionsDB;
        private readonly TMP_Text _label; // optionnel (libellé localisé "Limite FPS")

        private readonly List<FrameRateParams> _options = new(); // index -> option

        public FrameRateLimitSetting Setting => _setting;

        public FrameRateDropdownController(
            TMP_Dropdown dropdown,
            FrameRateLimitSetting setting,
            LocalizedStringDatabase_Options_Display optionsDisplayDB,
            TMP_Text localizedLabel = null)
        {
            _dropdown = dropdown;
            _setting = setting;
            _optionsDB = optionsDisplayDB;
            _label = localizedLabel;
        }

        /// <summary>Initialisation complète (liste, sélection, listeners, label).</summary>
        public void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError("[FrameRateDropdownController] TMP_Dropdown is null!");
                return;
            }

            BuildOptions();     // construit _options avec la bonne traduction d'“Illimité”
            RebuildDropdown();  // pousse _options dans _dropdown

            // Choisit l’index (exact si possible, sinon le plus proche)
            int idx = IndexFromCurrentTarget();
            _dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            _dropdown.value = Mathf.Clamp(idx, 0, _dropdown.options.Count - 1);
            _dropdown.RefreshShownValue();
            _dropdown.onValueChanged.AddListener(OnDropdownChanged);

            // Pousse vers le modèle
            UpdateSettingFromIndex(_dropdown.value);

            if (_label != null)
                _label.text = _optionsDB.FPS_Limit.GetLocalizedString();
        }

        /// <summary>À appeler quand la langue change.</summary>
        public void UpdateLanguage()
        {
            if (_label != null)
                _label.text = _optionsDB.FPS_Limit.GetLocalizedString();

            // On reconstruit juste les textes (notamment “Illimité”)
            int prev = _dropdown.value;
            BuildOptions();
            RebuildDropdown();
            _dropdown.SetValueWithoutNotify(Mathf.Clamp(prev, 0, _dropdown.options.Count - 1));
            _dropdown.RefreshShownValue();
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
                int idx = IndexFromCurrentTarget();
                _dropdown.SetValueWithoutNotify(Mathf.Clamp(idx, 0, _dropdown.options.Count - 1));
                _dropdown.RefreshShownValue();
                UpdateSettingFromIndex(_dropdown.value);
            }
        }

        // ---------- Internals ----------

        private void BuildOptions()
        {
            _options.Clear();

            // Valeurs classiques + “Illimité”
            _options.Add(new FrameRateParams(FormatFpsLabel(30), 30));
            _options.Add(new FrameRateParams(FormatFpsLabel(60), 60));
            _options.Add(new FrameRateParams(FormatFpsLabel(120), 120));
            _options.Add(new FrameRateParams(FormatFpsLabel(240), 240));

            string unlimited = _optionsDB.FPS_Unlimited.GetLocalizedString(); // ex: "Unlimited"
            _options.Add(new FrameRateParams(unlimited, -1));
        }

        private void RebuildDropdown()
        {
            var opts = new List<TMP_Dropdown.OptionData>(_options.Count);
            for (int i = 0; i < _options.Count; i++)
                opts.Add(new TMP_Dropdown.OptionData(_options[i].Name));

            _dropdown.options = opts;
        }

        private static string FormatFpsLabel(int fps)
        {
            // "60 FPS" (culture invariante pour être safe, même si pas indispensable ici)
            return $"{fps.ToString(CultureInfo.InvariantCulture)} FPS";
        }

        private int IndexFromCurrentTarget()
        {
            // NOTE: si VSync est actif, Application.targetFrameRate peut être ignoré,
            // mais on se contente d’afficher la valeur sauvegardée/modèle ou Application.targetFrameRate.
            int current = Application.targetFrameRate; // -1 si illimité/défaut

            // Essaye de trouver un match exact
            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i].Rate == current)
                    return i;
            }

            // Sinon, prend la plus proche > 0, sinon l’option illimitée
            if (current > 0)
            {
                int bestIdx = 0;
                int bestDelta = Mathf.Abs(_options[0].Rate - current);
                for (int i = 1; i < _options.Count; i++)
                {
                    if (_options[i].Rate < 0) continue; // ignore illimité pour la recherche de proximité
                    int d = Mathf.Abs(_options[i].Rate - current);
                    if (d < bestDelta)
                    {
                        bestDelta = d;
                        bestIdx = i;
                    }
                }
                return bestIdx;
            }

            // -1 ou autre: index de l’illimité
            for (int i = 0; i < _options.Count; i++)
                if (_options[i].Rate < 0) return i;

            return 0;
        }

        private void OnDropdownChanged(int index)
        {
            UpdateSettingFromIndex(index);
        }

        private void UpdateSettingFromIndex(int index)
        {
            if (index < 0 || index >= _options.Count) return;
            _setting.SetSelectedFrameRate(_options[index].Rate);
        }
    }
}
