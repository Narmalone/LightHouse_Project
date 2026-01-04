using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Core/Settings/Audio", "Audio", order: 1)]
    public sealed class AudioSettingsPage : IToolPage
    {
        private ConfigAssetPanel<AudioSettingsConfig> _audioSettingsConfig;

        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();

            _audioSettingsConfig ??= new ConfigAssetPanel<AudioSettingsConfig>(
                title: "Audio Settings",
                objectFieldLabel: "Audio Settings Config",
                defaultFolder: "Assets/Configs/Settings",
                defaultFileName: "SO_OceanGlobalSetting"
            );

            _audioSettingsConfig.AttachTo(root);

        }
    }
}