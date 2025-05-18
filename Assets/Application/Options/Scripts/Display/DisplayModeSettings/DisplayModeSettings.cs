using UnityEngine;

namespace LightHouse.Game.Options
{
    public class DisplayModeSetting : IOptionSetting
    {
        private FullScreenMode initialMode;
        private FullScreenMode currentMode;
        private FullScreenMode selectedMode;

        public DisplayModeSetting()
        {
            initialMode = Screen.fullScreenMode;
            currentMode = initialMode;
            selectedMode = initialMode;
        }

        public void SetSelectedMode(FullScreenMode mode)
        {
            selectedMode = mode;
        }

        public bool HasChanged()
        {
            return currentMode != selectedMode;
        }

        public void Apply()
        {
            if (currentMode != selectedMode)
            {
                currentMode = selectedMode;

                // Forcer la résolution actuelle mais avec le nouveau mode d'affichage
                Screen.SetResolution(ResolutionSetting.CurrentResolution.x, ResolutionSetting.CurrentResolution.y, selectedMode);
                Debug.Log($"Display mode applied: {selectedMode}");
            }
        }


        public void Revert()
        {
            Screen.fullScreenMode = currentMode;
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
