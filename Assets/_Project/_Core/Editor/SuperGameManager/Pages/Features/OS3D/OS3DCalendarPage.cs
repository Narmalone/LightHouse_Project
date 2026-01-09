using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/OS3D/Calendar", "Calendar", order: 1)]
    public sealed class OS3DCalendarPage : IToolPage
    {
        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();
            root.Add(new Label("Calendar config (TODO)"));
        }
    }
}