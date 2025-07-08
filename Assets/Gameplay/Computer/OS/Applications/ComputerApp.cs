using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.OS
{
    public enum E_ComputerAppState
    {
        None,
        Opened,
        Closed,
        Minimized
    }

    public abstract class ComputerApp : MonoBehaviour
    {
        public E_ComputerAppState State;
        [field: SerializeField] public string AppName { get; protected set; }
        [SerializeField] protected Button _closeButton;
        [SerializeField] private RectTransform _rectTransform;
        public bool IsMinimized { get; private set; }

        protected OS _os;
        public RectTransform RectTransform => _rectTransform;
        public abstract void OnOpen();
        public abstract void OnClose();
        public abstract void OnMinimize();

        protected virtual void Awake()
        {
            _closeButton.onClick.AddListener(OnClose);
        }

        protected virtual void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnClose);
        }

        public virtual void Initialize(OS os)
        {
            _os = os;
        }

        public void ToggleMinimize()
        {
            IsMinimized = !IsMinimized;
            OnMinimize();
        }
    }
}

