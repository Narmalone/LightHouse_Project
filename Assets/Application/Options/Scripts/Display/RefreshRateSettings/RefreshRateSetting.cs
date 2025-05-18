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

        public void SetSelectedRefreshRate(float refreshRate)
        {
            selectedRefreshRate = new RefreshRate
            {
                numerator = (uint)(refreshRate * 1000), // ou mieux : stocker float/double proprement
                denominator = 1000
            };
        }


        public uint GetCurrentSelectedRefreshRate()
        {
            return (uint)selectedRefreshRate.value;
        }

        public bool HasChanged()
        {
            return (float)Mathf.Abs((float)initialRefreshRate.value - (float)selectedRefreshRate.value) > 0.5f;
        }

        public void Apply()
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, Screen.fullScreenMode, selectedRefreshRate);
            initialRefreshRate = selectedRefreshRate;
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
