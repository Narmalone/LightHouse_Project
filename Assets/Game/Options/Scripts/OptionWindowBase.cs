using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    [System.Serializable]
    public abstract class OptionWindowBase
    {
        protected VisualElement root;
        protected ConfirmationPopupController confirmationPopupController;

        public OptionWindowBase(VisualElement rootElement, ConfirmationPopupController confirmationPopUp)
        {
            root = rootElement;
            confirmationPopupController = confirmationPopUp;
        }

        protected OptionWindowBase(VisualElement root)
        {
            this.root = root;
        }

        public abstract void InitializeControllers();
        public abstract void ApplySettings();
        public abstract void RevertSettings();
        public virtual void Show() => root.style.display = DisplayStyle.Flex;
        public virtual void Hide() => root.style.display = DisplayStyle.None;

        public abstract bool HasChanges();
    }

}
