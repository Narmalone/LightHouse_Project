using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Features/OS3D", "OS3D", order: 0)]
    public sealed class OS3DPage : IToolPage
    {
        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();

            // Title
            var title = new Label("OS3D");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16;
            title.style.marginBottom = 6;
            root.Add(title);

            // Small description / overview
            var desc = new Label("Configure les systèmes OS3D. Utilise les sous-catégories ci-dessous.");
            desc.style.opacity = 0.85f;
            desc.style.marginBottom = 10;
            root.Add(desc);

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
