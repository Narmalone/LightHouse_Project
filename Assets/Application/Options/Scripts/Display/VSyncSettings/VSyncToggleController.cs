using LightHouse.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    public class VSyncToggleController
    {
        private readonly Toggle toggle;
        private readonly VSyncSetting setting;
        private LocalizedStringDatabase_Options_Display _optionsDB;

        public VSyncSetting Setting => setting;

        public VSyncToggleController(Toggle toggle, VSyncSetting setting, LocalizedStringDatabase_Options_Display optionsDB)
        {
            this.toggle = toggle;
            this.setting = setting;
            this._optionsDB = optionsDB;
        }

        public void Initialize()
        {
            if (toggle == null)
            {
                Debug.LogError("Toggle is null for VSyncToggleController!");
                return;
            }

            toggle.value = QualitySettings.vSyncCount > 0;
            setting.SetSelectedVSync(toggle.value);

            toggle.RegisterValueChangedCallback(evt => setting.SetSelectedVSync(evt.newValue));
        }

        public void UpdateLanguage()
        {
            toggle.label = _optionsDB.VSync.GetLocalizedString();
        }

        public void Apply()
        {
            if (setting.HasChanged()) setting.Apply();
        }

        public void Revert()
        {
            if (setting.HasChanged())
            {
                setting.Revert();
                toggle.SetValueWithoutNotify(QualitySettings.vSyncCount > 0);
            }
        }
    }
}
