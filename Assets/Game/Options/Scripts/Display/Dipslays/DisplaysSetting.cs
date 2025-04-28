using UnityEngine;

namespace LightHouse.Game.Options
{
    public class DisplaysSetting : IOptionSetting
    {
        private int initialDisplay;
        private int currentDisplay;
        private int selectedDisplay;

        public int SelectedDisplay => selectedDisplay;

        public DisplaysSetting()
        {
            initialDisplay = 0; // Toujours Display 0 (Display 1 dans Unity)
            currentDisplay = initialDisplay;
            selectedDisplay = initialDisplay;
        }

        public void SetSelectedDisplay(int displayIndex)
        {
            selectedDisplay = displayIndex;
        }

        public bool HasChanged()
        {
            return currentDisplay != selectedDisplay;
        }

        public void Apply()
        {
            if (selectedDisplay < Display.displays.Length)
            {
                Display.displays[selectedDisplay].Activate();
                currentDisplay = selectedDisplay;
                Debug.Log($"Display {selectedDisplay + 1} activé !");
            }
            else
            {
                Debug.LogWarning("Display index hors limites !");
            }
        }

        public void Revert()
        {
            Display.displays[initialDisplay].Activate();
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
