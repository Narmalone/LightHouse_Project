using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace LightHouse.Game.Options
{
    public class DisplaysDropdownController
    {
        private readonly DropdownField dropdown;
        private readonly ConfirmationPopupController confirmationPopupController;

        public DisplaysDropdownController(DropdownField dropdown, ConfirmationPopupController confirmationPopupController)
        {
            this.dropdown = dropdown;
            this.confirmationPopupController = confirmationPopupController;
        }

        public void Initialize()
        {
            RefreshDropdown();
            dropdown.RegisterValueChangedCallback(evt => OnDisplayChanged(evt.newValue));
        }

        private void RefreshDropdown()
        {
            List<string> displays = new();
            List<DisplayInfo> displayInfos = new();
            Screen.GetDisplayLayout(displayInfos);

            for (int i = 0; i < displayInfos.Count; i++)
            {
                displays.Add($"Display {i + 1} ({displayInfos[i].width}x{displayInfos[i].height})");
            }

            dropdown.choices = displays;
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
    }
}
