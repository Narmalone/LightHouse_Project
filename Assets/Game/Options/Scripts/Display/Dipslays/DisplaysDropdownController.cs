using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Localization;
using LightHouse.UIExtensions;
using System;

namespace LightHouse.Game.Options
{
    public class DisplaysDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly ConfirmationPopupController confirmationPopupController;
        private LocalizedString _displayName;

        private DisplaysSetting DisplaysSetting;

        public DisplaysDropdownController(DropdownField dropdown, ConfirmationPopupController confirmationPopupController, LocalizedString displayName)
        {
            this.dropdown = dropdown;
            this.confirmationPopupController = confirmationPopupController;
            this._displayName = displayName;
            DisplaysSetting = new DisplaysSetting();
        }

        public void Initialize()
        {
            RefreshDropdown();
            dropdown.RegisterValueChangedCallback(evt => OnDisplayChanged(evt.newValue));
        }

        public void UpdateLanguage()
        {
            RefreshDropdown();
        }

        private void RefreshDropdown()
        {
            List<string> displays = new();
            List<DisplayInfo> displayInfos = new();
            Screen.GetDisplayLayout(displayInfos);

            for (int i = 0; i < displayInfos.Count; i++)
            {
                displays.Add($"{_displayName.GetLocalizedString()} {i + 1} ({displayInfos[i].width}x{displayInfos[i].height})");
            }

            dropdown.UpdateChoices(displays);
            dropdown.SetValueWithoutNotify(displays[DisplaySettingManager.GetCurrentDisplayIndex()]);
        }

        private void OnDisplayChanged(string newValue)
        {
            int index = dropdown.choices.IndexOf(newValue);
            if (index >= 0)
            {
                DisplaySettingManager.ApplyDisplayChange(index);

                confirmationPopupController.Show(
                    confirmAction: () => { Debug.Log("Display change confirmed."); },
                    cancelAction: () => { DisplaySettingManager.RevertDisplayChange(); },
                    timeOutAction: 15
                );
            }
        }

        internal void Apply()
        {
            if(DisplaysSetting.HasChanged())
                DisplaysSetting.Apply();
        }
    }
}
