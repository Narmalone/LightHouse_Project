// MonitorsEnumController.cs (version simplifiée/uniforme)
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Options
{
    public class MonitorsEnumController
    {
      /*  private readonly OptionEnum _optionsEnum;
        private readonly TMP_Text _label;
        private readonly LocalizedString _displayName;
        private readonly MonitorSetting _setting; // <- injecté
        private readonly List<DisplayInfo> _displayInfos = new();

        public MonitorSetting Setting => _setting;

        public MonitorsEnumController(OptionEnum optionsEnum,
                                      MonitorSetting setting,
                                      LocalizedString displayName,
                                      TMP_Text localizedLabel = null)
        {
            _optionsEnum = optionsEnum;
            _setting = setting;
            _displayName = displayName;
            _label = localizedLabel;
        }

        public void Initialize()
        {
            if (_optionsEnum == null) { Debug.LogError("[MonitorsEnum] OptionEnum null"); return; }

            RebuildOptions();

            int currentIdx = Mathf.Clamp(MonitorSetting.GetCurrentDisplayIndex(), 0, _optionsEnum.Choices.Count - 1);
            SafeSetSelectedIndex(currentIdx);
            _setting.SetSelectedDisplay(currentIdx);

            // écoute user → ne fait QUE setter le modèle
            _optionsEnum.OnValueChanged -= OnOptionChanged;
            _optionsEnum.OnValueChanged += OnOptionChanged;

            // refléter après Apply/Revert système
            MonitorSetting.OnDisplayScreenChanged -= OnSystemChanged;
            MonitorSetting.OnDisplayScreenChanged += OnSystemChanged;

            if (_label != null && _displayName != null)
                _label.text = _displayName.GetLocalizedString();
        }

        public void Detach()
        {
            _optionsEnum.OnValueChanged -= OnOptionChanged;
            MonitorSetting.OnDisplayScreenChanged -= OnSystemChanged;
        }

        private void OnOptionChanged(int newIndex)
        {
            _setting.SetSelectedDisplay(newIndex); // pas d’Apply ici
        }

        private void OnSystemChanged()
        {
            int idx = Mathf.Clamp(MonitorsSetting.GetCurrentDisplayIndex(), 0, _optionsEnum.Choices.Count - 1);
            SafeSetSelectedIndex(idx);
            _setting.SetSelectedDisplay(idx);
            RebuildOptions();
        }

        public void RebuildOptions()
        {
            _displayInfos.Clear();
            Screen.GetDisplayLayout(_displayInfos);

            var options = new List<string>();
            string baseName = "Display";

            for (int i = 0; i < _displayInfos.Count; i++)
                options.Add($"{baseName} {i + 1} ({_displayInfos[i].width}x{_displayInfos[i].height})");

            if (options.Count == 0)
                options.Add($"{baseName} 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})");

            _optionsEnum.Choices.Clear();
            _optionsEnum.AddOptions(options);
        }

        private void SafeSetSelectedIndex(int index)
        {
            // SetSelectedIndex si dispo, sinon CurrentChoiceIndex + ForceRebuildUI
            try { _optionsEnum.SetSelectedIndex(index); }
            catch { _optionsEnum.CurrentChoiceIndex = index; _optionsEnum.ForceRebuildUI(); }
        }*/
    }
}
