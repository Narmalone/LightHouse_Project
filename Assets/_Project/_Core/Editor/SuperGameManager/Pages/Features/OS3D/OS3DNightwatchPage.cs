using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/OS3D/Nightwatch", "Nightwatch", order: 0)]
    public sealed class OS3DNightwatchPage : IToolPage
    {
        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();
            root.Add(new Label("Nightwatch config (TODO)"));
        }
    }
}

