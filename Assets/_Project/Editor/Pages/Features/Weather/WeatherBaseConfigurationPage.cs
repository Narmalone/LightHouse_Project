using LightHouse.Weather;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/Weather/_Global Config", "Global Config", order: 0)]
    public sealed class WeatherBaseConfigurationPage : IToolPage
    {
        private ConfigAssetPanel<WeatherConfigDatabase> _weatherConfigDatabase;
        private ConfigAssetPanel<WeatherConfiguration> _weatherConfig;

        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();

            _weatherConfigDatabase ??= new ConfigAssetPanel<WeatherConfigDatabase>(
                title: "Weather Config Database",
                objectFieldLabel: "Weather Database",
                defaultFolder: "Assets/Configs/Weather",
                defaultFileName: "SO_WeatherDatabase_"
            );

            _weatherConfig ??= new ConfigAssetPanel<WeatherConfiguration>(
                title: "Weather Config",
                objectFieldLabel: "Weather Config",
                defaultFolder: "Assets/Configs/Weather",
                defaultFileName: "SO_Weather_Config_"
            );

            _weatherConfigDatabase.AttachTo(root);
            _weatherConfig.AttachTo(root);
        }
    }
}
