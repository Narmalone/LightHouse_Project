using LightHouse.Features.Computer.LEO.Weather;
using LightHouse.Features.Weather.Ocean;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/Weather/Ocean", "Ocean", order: 0)]
    public sealed class WeatherOceanPage : IToolPage
    {
        private ConfigAssetPanel<OceanConfiguration> _oceanGlobalSettingsPanel;
        private ConfigAssetPanel<SO_BeaufortScale> _beaufortPanel;

        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();

            _oceanGlobalSettingsPanel ??= new ConfigAssetPanel<OceanConfiguration>(
                title: "Weather Ocean",
                objectFieldLabel: "Global settings",
                defaultFolder: "Assets/Configs/Weather",
                defaultFileName: "SO_OceanGlobalSetting"
            );

            _beaufortPanel ??= new ConfigAssetPanel<SO_BeaufortScale>(
                title: "Beaufort Scale",
                objectFieldLabel: "BeaufortScale Datas",
                defaultFolder: "Assets/Configs/Weather",
                defaultFileName: "SO_BeaufortScale"
            );

            _oceanGlobalSettingsPanel.AttachTo(root);
            _beaufortPanel.AttachTo(root);
        }
    }
}
