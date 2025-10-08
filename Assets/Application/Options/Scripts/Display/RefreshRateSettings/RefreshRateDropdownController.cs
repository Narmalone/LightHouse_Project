using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// TMP_Dropdown (uGUI) pour le taux de rafraîchissement (Hz).
    /// - Déduplique par tolérance (~0.01 Hz)
    /// - Formate en entier si ~int, sinon 2 décimales
    /// - Sélectionne l’index le plus proche du courant
    /// </summary>
    public class RefreshRateDropdownController
    {
        private readonly TMP_Dropdown _dropdown;
        private readonly RefreshRateSetting _setting;
        private readonly LocalizedStringDatabase_Options_Display _optionsDB;
        private readonly TMP_Text _label; // optionnel

        private readonly List<float> _ratesHz = new(); // mapping index -> Hz

        // tolérance pour comparer deux Hz considérés identiques
        private const float kEpsilon = 0.01f;

        public RefreshRateSetting Setting => _setting;

        public RefreshRateDropdownController(
            TMP_Dropdown dropdown,
            RefreshRateSetting setting,
            LocalizedStringDatabase_Options_Display optionsDb,
            TMP_Text localizedLabel = null)
        {
            _dropdown = dropdown;
            _setting = setting;
            _optionsDB = optionsDb;
            _label = localizedLabel;
        }

        public void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError("[RefreshRateDropdown] TMP_Dropdown is null!");
                return;
            }

            RebuildRates();

            // sélectionne l’index le plus proche du refresh courant
            float currentHz = (float)Screen.currentResolution.refreshRateRatio.value;
            int idx = IndexOfClosest(currentHz);

            _dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            _dropdown.options = BuildOptions(_ratesHz);
            _dropdown.value = Mathf.Clamp(idx, 0, _dropdown.options.Count - 1);
            _dropdown.RefreshShownValue();
            _dropdown.onValueChanged.AddListener(OnDropdownChanged);

            // pousse vers le modèle
            UpdateSettingFromIndex(_dropdown.value);

            if (_label != null)
                _label.text = _optionsDB.Refresh_Rate.GetLocalizedString();
        }

        public void UpdateLanguage()
        {
            if (_label != null)
                _label.text = _optionsDB.Refresh_Rate.GetLocalizedString();
            // Les options restent identiques (nombres). Rien à reconstruire ici.
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

                // Re-sync avec l’état actuel du système
                float currentHz = (float)Screen.currentResolution.refreshRateRatio.value;
                int idx = IndexOfClosest(currentHz);
                _dropdown.SetValueWithoutNotify(Mathf.Clamp(idx, 0, _dropdown.options.Count - 1));
                _dropdown.RefreshShownValue();
                UpdateSettingFromIndex(_dropdown.value);
            }
        }

        // ---------- Internals ----------

        private void RebuildRates()
        {
            _ratesHz.Clear();

            int maxWidth = Display.main != null ? Display.main.systemWidth : Screen.currentResolution.width;
            int maxHeight = Display.main != null ? Display.main.systemHeight : Screen.currentResolution.height;

            foreach (var r in Screen.resolutions)
            {
                if (r.width > maxWidth || r.height > maxHeight) continue;

                float hz = (float)r.refreshRateRatio.value;
                if (!_ratesHz.Any(x => Mathf.Abs(x - hz) < kEpsilon))
                    _ratesHz.Add(hz);
            }

            // Garantir la présence du courant
            float currentHz = (float)Screen.currentResolution.refreshRateRatio.value;
            if (!_ratesHz.Any(x => Mathf.Abs(x - currentHz) < kEpsilon))
                _ratesHz.Add(currentHz);

            _ratesHz.Sort();
        }

        private List<TMP_Dropdown.OptionData> BuildOptions(List<float> rates)
        {
            var opts = new List<TMP_Dropdown.OptionData>(rates.Count);
            for (int i = 0; i < rates.Count; i++)
            {
                string label = FormatHz(rates[i]);
                opts.Add(new TMP_Dropdown.OptionData(label));
            }
            return opts;
        }

        private static string FormatHz(float hz)
        {
            // 59.94 doit afficher 59.94 ; 60.0 ~ "60 Hz"
            string s = (Mathf.Abs(hz - Mathf.Round(hz)) < 0.05f)
                ? $"{Mathf.RoundToInt(hz).ToString(CultureInfo.InvariantCulture)} Hz"
                : $"{hz.ToString("0.00", CultureInfo.InvariantCulture)} Hz";
            return s;
        }

        private int IndexOfClosest(float targetHz)
        {
            if (_ratesHz.Count == 0) return 0;

            int best = 0;
            float bestDelta = Mathf.Abs(_ratesHz[0] - targetHz);
            for (int i = 1; i < _ratesHz.Count; i++)
            {
                float d = Mathf.Abs(_ratesHz[i] - targetHz);
                if (d < bestDelta)
                {
                    best = i; bestDelta = d;
                }
            }
            return best;
        }

        private void OnDropdownChanged(int index)
        {
            UpdateSettingFromIndex(index);
        }

        private void UpdateSettingFromIndex(int index)
        {
            if (index < 0 || index >= _ratesHz.Count) return;
            _setting.SetSelectedRefreshRate(_ratesHz[index]);
        }
    }
}
