using UnityEngine;

namespace LightHouse.Features.Computer.LEO.NightWatch
{
    public abstract class NightWatchReportWindow : MonoBehaviour
    {
        [SerializeField] protected E_NightWatchMode _windowType;
        [SerializeField] protected NightWatchController _nightWatch;
        [SerializeField] protected CanvasGroup _canvasGroup;
        public E_NightWatchMode WindowType => _windowType;

        public void SetNightWatch(NightWatchController nightWatch)
        {
            _nightWatch = nightWatch;
        }

        public virtual void Open()
        {
            _canvasGroup.alpha = 1.0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            //gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            _canvasGroup.alpha = 0.0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            //gameObject.SetActive(false);
        }
    }
}

