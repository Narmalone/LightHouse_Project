using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LightHouse.Localization;

namespace LightHouse.Game.Options
{
    public class DisplayModeOptionEnumController
    {
        private readonly OptionEnum _optionEnum;
        private readonly DisplayModeSetting _setting;
        private readonly LocalizedStringDatabase_Options_Display _optionsDB;
        private readonly TMP_Text _label;

        private readonly List<DisplayModeOption> _order = new()
        {
            DisplayModeOption.Fullscreen,
            DisplayModeOption.Borderless,
            DisplayModeOption.MaximizedWindow,
            DisplayModeOption.Windowed
        };

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

        public void Initialize()
        {
            if (_optionEnum == null)
            {
                Debug.LogError("[DisplayModeOptionEnumController] OptionEnum est null.");
                return;
            }

            BuildLocalizedLabels();

            var choices = new List<string>(_order.Count);
            foreach (var opt in _order) choices.Add(_labels[opt]);
            _optionEnum.Choices = choices;

            int currentIndex = IndexFromUnityMode(Screen.fullScreenMode);
            SafeSetSelectedIndex(currentIndex);          // <<< évite le problème de méthode privée ou d’event

            UpdateSettingFromIndex(currentIndex);

            // Si ton OptionEnum expose OnValueChanged(int), on s’y abonne ; sinon, utilise les boutons
            try
            {
                _optionEnum.OnValueChanged -= OnOptionChanged;
                _optionEnum.OnValueChanged += OnOptionChanged;
            }
            catch
            {
                _optionEnum.LeftButton.onClick.AddListener(() => OnOptionChanged(_optionEnum.CurrentChoiceIndex));
                _optionEnum.RightButton.onClick.AddListener(() => OnOptionChanged(_optionEnum.CurrentChoiceIndex));
            }

            if (_label != null)
                _label.text = _optionsDB.Display_Mode.GetLocalizedString();
        }

        public void UpdateLanguage()
        {
            if (_label != null)
                _label.text = _optionsDB.Display_Mode.GetLocalizedString();

            int prevIndex = Mathf.Clamp(_optionEnum.CurrentChoiceIndex, 0, _order.Count - 1);

            BuildLocalizedLabels();

            var choices = new List<string>(_order.Count);
            foreach (var opt in _order) choices.Add(_labels[opt]);
            _optionEnum.Choices = choices;

            SafeSetSelectedIndex(prevIndex);            // <<< idem ici
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
                SafeSetSelectedIndex(idx);
                UpdateSettingFromIndex(idx);
            }
        }

        public void Detach()
        {
            if (_optionEnum == null) return;
            try { _optionEnum.OnValueChanged -= OnOptionChanged; } catch { /* ignore */ }
            _optionEnum.LeftButton.onClick.RemoveAllListeners();
            _optionEnum.RightButton.onClick.RemoveAllListeners();
        }

        public bool HasChanges() => _setting.HasChanged();

        // ---------- Internes ----------

        private void OnOptionChanged(int newIndex) => UpdateSettingFromIndex(newIndex);

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

            // ✅ FIX 1: passer le bon paramètre et la bonne méthode
            _setting.SetSelected((global::DisplayModeOption)unityMode);
            // (ou _setting.SetSelected(mode); si ton Setting attend l’enum maison et non le FullScreenMode)
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

        // ✅ FIX 2: SetSelectedIndex peut être privé → fallback propre
        private void SafeSetSelectedIndex(int index)
        {
            try
            {
                _optionEnum.SetSelectedIndex(index); // s'il est public chez toi
            }
            catch
            {
                _optionEnum.CurrentChoiceIndex = index;
                _optionEnum.ForceRebuildUI();
            }
        }
    }
}
