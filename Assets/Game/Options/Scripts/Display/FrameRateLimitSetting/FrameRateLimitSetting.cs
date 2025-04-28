using UnityEngine;

namespace LightHouse.Game.Options
{
    public class FrameRateLimitSetting : IOptionSetting
    {
        private int initialTargetFPS;
        private int selectedTargetFPS;

        public FrameRateLimitSetting()
        {
            initialTargetFPS = Application.targetFrameRate;
            selectedTargetFPS = initialTargetFPS;
        }

        public void SetSelectedFrameRate(int fps)
        {
            selectedTargetFPS = fps;
        }

        public bool HasChanged()
        {
            return initialTargetFPS != selectedTargetFPS;
        }

        public void Apply()
        {
            Application.targetFrameRate = selectedTargetFPS;
            initialTargetFPS = selectedTargetFPS;
            Debug.Log("frame limit applied");
        }

        public void Revert()
        {
            Application.targetFrameRate = initialTargetFPS;
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
