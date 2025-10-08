using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.Game.Options
{
    [System.Serializable]
    public abstract class OptionWindowBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        protected ConfirmationPopupController confirmationPopupController;
        protected IOptionSetting[] optionSettings;

        public abstract void InitializeControllers();
        public abstract void ApplySettings();
        public abstract void RevertSettings();
        public virtual void Show()
        {
            _canvasGroup.alpha = 1.0f;
            _canvasGroup.interactable = true;
        }
        public virtual void Hide()
        {
            _canvasGroup.alpha = 0.0f;
            _canvasGroup.interactable = false;
        }

        public abstract bool HasChanges();
    }

}
