using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization;

namespace LightHouse.Game.Options
{
    /// <summary>
    /// Contrôle un OptionEnum listant les écrans disponibles.
    /// </summary>
    public class MonitorsEnumController
    {
        private readonly OptionEnum _optionsEnum;
        private readonly ConfirmationPopupController _popup;
        private readonly LocalizedString _displayName;
        private readonly TMP_Text _label;
        private readonly DisplaysSetting _displaysSetting;
        private readonly List<DisplayInfo> _displayInfos = new();

        public MonitorsEnumController(
            OptionEnum optionsEnum,
            ConfirmationPopupController confirmationPopupController,
            LocalizedString displayName,
            TMP_Text localizedLabel = null)
        {
            _optionsEnum = optionsEnum;
            _popup = confirmationPopupController;
            _displayName = displayName;
            _label = localizedLabel;
            _displaysSetting = new DisplaysSetting();
        }

        public void Initialize()
        {
            if (_optionsEnum == null)
            {
                Debug.LogError("[MonitorsEnumController] OptionEnum est null.");
                return;
            }

            RebuildOptions();

            // Sélection de l’écran courant
            int currentIndex = Mathf.Clamp(DisplaySettingManager.GetCurrentDisplayIndex(), 0, _optionsEnum.Choices.Count - 1);
            _optionsEnum.SetSelectedIndex(currentIndex);

            // État initial
            UpdateSettingFromIndex(currentIndex);

            // Branche l’événement
            _optionsEnum.OnValueChanged -= OnOptionChanged;
            _optionsEnum.OnValueChanged += OnOptionChanged;
        }

        public void UpdateLanguage()
        {
            if (_label != null && _displayName != null)
                _label.text = _displayName.GetLocalizedString();

            int prevIndex = _optionsEnum.CurrentChoiceIndex;
            RebuildOptions();
            _optionsEnum.SetSelectedIndex(Mathf.Clamp(prevIndex, 0, _optionsEnum.Choices.Count - 1));
        }

        public void Apply()
        {
            if (_displaysSetting.HasChanged())
                _displaysSetting.Apply();
        }

        private void RebuildOptions()
        {
            _displayInfos.Clear();
            Screen.GetDisplayLayout(_displayInfos);

            List<string> options = new();
            string baseName = _displayName != null ? _displayName.GetLocalizedString() : "Display";

            for (int i = 0; i < _displayInfos.Count; i++)
            {
                var info = _displayInfos[i];
                options.Add($"{baseName} {i + 1} ({info.width}x{info.height})");
            }

            if (options.Count == 0)
            {
                options.Add($"{baseName} 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})");
            }

            _optionsEnum.Choices.Clear();
            _optionsEnum.AddOptions(options);
        }

        private void OnOptionChanged(int newIndex)
        {
            int current = Mathf.Clamp(DisplaySettingManager.GetCurrentDisplayIndex(), 0, _optionsEnum.Choices.Count - 1);
            if (newIndex == current)
            {
                UpdateSettingFromIndex(newIndex);
                return;
            }

            DisplaySettingManager.ApplyDisplayChange(newIndex);
            UpdateSettingFromIndex(newIndex);

            if (_popup != null)
            {
                _popup.Show(
                    confirmAction: () =>
                    {
                        Debug.Log("Display change confirmed.");
                    },
                    cancelAction: () =>
                    {
                        DisplaySettingManager.RevertDisplayChange();
                        int idx = Mathf.Clamp(DisplaySettingManager.GetCurrentDisplayIndex(), 0, _optionsEnum.Choices.Count - 1);
                        _optionsEnum.SetSelectedIndex(idx);
                        UpdateSettingFromIndex(idx);
                    },
                    timeOutAction: 15
                );
            }
        }

        private void UpdateSettingFromIndex(int index)
        {
            _displaysSetting.SetSelectedDisplay(index);
        }
    }
}
