using LightHouse.Features.Weather.Clouds.Settings;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/Weather/Clouds", "Clouds", order: 0)]
    public sealed class WeatherCloudPage : IToolPage
    {
        private ConfigAssetPanel<CloudSettings> _panel;

        public void Build(VisualElement root, NavContext ctx)
        {
            _panel ??= new ConfigAssetPanel<CloudSettings>(
                title: "Weather Clouds",
                objectFieldLabel: "CloudSettings Asset",
                defaultFolder: "Assets/Configs/Weather",
                defaultFileName: "CloudSettings"
            );

            _panel.AttachTo(root);
        }
    }
}
