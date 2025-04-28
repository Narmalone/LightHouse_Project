using UnityEngine;

namespace LightHouse.Game.Options
{
    public class RefreshRateSetting : IOptionSetting
    {
        private RefreshRate initialRefreshRate;
        private RefreshRate selectedRefreshRate;

        public RefreshRateSetting()
        {
            initialRefreshRate = Screen.currentResolution.refreshRateRatio;
            selectedRefreshRate = initialRefreshRate;
        }

        public void SetSelectedRefreshRate(uint refreshRate)
        {
            selectedRefreshRate.numerator = refreshRate;
        }

        public bool HasChanged()
        {
            return !initialRefreshRate.Equals(selectedRefreshRate);
        }

        public void Apply()
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, Screen.fullScreenMode, selectedRefreshRate);
            initialRefreshRate = selectedRefreshRate;
            Debug.Log("refresh rate applied");
        }

        public void Revert()
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, Screen.fullScreenMode, initialRefreshRate);
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
