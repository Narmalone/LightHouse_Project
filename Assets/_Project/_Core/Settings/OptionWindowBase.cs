using UnityEngine;

namespace LightHouse.Core.Settings
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
            _canvasGroup.blocksRaycasts = true;
        }
        public virtual void Hide()
        {
            _canvasGroup.alpha = 0.0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public abstract bool HasChanges();
    }

}
