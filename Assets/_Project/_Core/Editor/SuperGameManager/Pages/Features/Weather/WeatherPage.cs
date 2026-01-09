using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/Weather", "Weather", order: 0)]
    public sealed class WeatherPage : IToolPage
    {
        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();
            root.Add(new Label("Weather settings (TODO)"));
/*
            var back = new Button(() => ctx.RequestBack?.Invoke()) { text = "Back" };
            back.style.marginTop = 10;
            root.Add(back);
*/

            // Sub-pages buttons
            var children = ctx.Current?.OrderedChildren();
            if (children == null)
            {
                root.Add(new HelpBox("Navigation context invalide (ctx.Current null).", HelpBoxMessageType.Warning));
                return;
            }

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            //grid.style.gap = 8;
            root.Add(grid);

            int count = 0;
            foreach (var child in children)
            {
                count++;

                var btn = new Button(() => ctx.RequestNavigate?.Invoke(child))
                {
                    text = child.DisplayName
                };
                btn.style.minWidth = 180;
                btn.style.height = 32;
                grid.Add(btn);
            }

            if (count == 0)
            {
                root.Add(new HelpBox(
                    "Aucune sous-page trouvée. Ajoute des pages avec des paths du type: Features/OS3D/Nightwatch",
                    HelpBoxMessageType.Info
                ));
            }
        }
    }
}
