using UnityEngine.UIElements;

namespace LightHouse.EditorTools.SuperGameManager
{
    public interface IToolPage
    {
        // Construit le contenu de la page dans le container fourni.
        void Build(VisualElement root, NavContext ctx);
    }
}
