using LightHouse.Features.TimeOfDay.Lighting;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/LightingProfiles", "LightingProfiles", order: 1)]
    public sealed class LightingPages : IToolPage
    {
        private ConfigAssetPanel<LightingProfile> _panel;

        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();
            _panel ??= new ConfigAssetPanel<LightingProfile>(
                title: "Lighting Profile",
                objectFieldLabel: "Lighting Profile Asset",
                defaultFolder: "Assets/Configs/Lighting",
                defaultFileName: "SO_X_X"
            );

            _panel.AttachTo(root);

        }
    }
}