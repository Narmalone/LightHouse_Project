using UnityEngine;

namespace LightHouse.Game.Options
{
    public class VSyncSetting : IOptionSetting
    {
        private int initialVSyncCount;
        private int currentVSyncCount;
        private int selectedVSyncCount;

        public VSyncSetting()
        {
            initialVSyncCount = QualitySettings.vSyncCount;
            currentVSyncCount = initialVSyncCount;
            selectedVSyncCount = initialVSyncCount;
        }

        public void SetSelectedVSync(bool enabled)
        {
            selectedVSyncCount = enabled ? 1 : 0;
        }

        public bool HasChanged()
        {
            return currentVSyncCount != selectedVSyncCount;
        }

        public void Apply()
        {
            QualitySettings.vSyncCount = selectedVSyncCount;
            currentVSyncCount = selectedVSyncCount;
            Debug.Log("Vsync applied");
        }

        public void Revert()
        {
            QualitySettings.vSyncCount = currentVSyncCount;
        }

        public IOptionSetting GetSetting()
        {
            return this;
        }
    }
}
