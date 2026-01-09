using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    public sealed class HubPage : IToolPage
    {
        private readonly NavNode _node;

        public HubPage(NavNode node)
        {
            _node = node;
        }

        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();

            var title = new Label(_node.DisplayName) { name = "sgm-title" };
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.fontSize = 16;
            title.style.marginBottom = 8;
            root.Add(title);

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            //grid.style.gap = 8;
            root.Add(grid);

            foreach (var child in _node.OrderedChildren())
            {
                var btn = new Button(() => ctx.RequestNavigate?.Invoke(child))
                {
                    text = child.DisplayName
                };
                btn.style.minWidth = 160;
                btn.style.height = 32;
                grid.Add(btn);
            }
        }
    }
}
