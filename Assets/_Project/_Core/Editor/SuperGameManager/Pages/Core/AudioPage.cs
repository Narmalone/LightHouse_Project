using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    [SgmPage("Core/Audio", "Audio", order: 0)]
    public sealed class AudioPage : IToolPage
    {
        public void Build(VisualElement root, NavContext ctx)
        {
            root.Clear();
            root.Add(new Label("Audio settings (TODO)"));

            var back = new Button(() => ctx.RequestBack?.Invoke()) { text = "Back" };
            back.style.marginTop = 10;
            root.Add(back);

            // Ici tu peux ajouter des toggles, sliders, object fields, etc.
        }
    }
}
