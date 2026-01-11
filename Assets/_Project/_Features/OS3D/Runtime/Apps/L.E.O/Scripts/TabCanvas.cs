using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.OS
{
    public class TabCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private GraphicRaycaster _raycaster;
        [SerializeField] private CanvasGroup _canvasGroup;

        public bool IsVisible => _canvasGroup.alpha > 0f;

        public void EnableCanvasGroup()
        {
            _canvasGroup.alpha = 1.0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void DisableCanvasGroup()
        {
            _canvasGroup.alpha = 0.0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void ShowCanvasGroup()
        {
            _canvasGroup.alpha = 1.0f;
        }

        public void HideCanvasGroup()
        {
            _canvasGroup.alpha = 0.0f;
        }
    }

}
