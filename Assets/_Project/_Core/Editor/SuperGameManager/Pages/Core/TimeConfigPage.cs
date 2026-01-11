using LightHouse.Features.TimeOfDay.TimeCore;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Core/Time", "Time", order: 10)]
    public sealed class TimeConfigPage : IToolPage
    {
        private ConfigAssetPanel<TimeConfiguration> _panel;

        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();
            _panel ??= new ConfigAssetPanel<TimeConfiguration>(
                title: "Time Config",
                objectFieldLabel: "TimeConfiguration Asset",
                defaultFolder: "Assets/Configs",
                defaultFileName: "TimeConfiguration"
            );

            _panel.AttachTo(root);
        }
    }
}
