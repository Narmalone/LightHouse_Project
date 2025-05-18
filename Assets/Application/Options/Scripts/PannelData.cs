using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    [System.Serializable]
    public class PanelData
    {
        public VisualElement Panel;
        public OptionWindowBase Window;

        public PanelData(VisualElement panel, OptionWindowBase window)
        {
            Panel = panel;
            Window = window;
        }
    }

}
