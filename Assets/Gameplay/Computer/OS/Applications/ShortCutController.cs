using UnityEngine;

namespace LightHouse.Game.Computer.OS
{
    public abstract class ShortCutController : MonoBehaviour
    {
        [SerializeField] private ComputerApp _prefab;
        protected OS _os;
        private RectTransform _runningAppsParent => _os.RunningAppParent;
        protected ComputerApp _currentInstance;

        [SerializeField] private CustomUIButton _customButton;

        protected virtual void Awake()
        {
            _customButton.OnDoubleClick += OnDoubleClicked;
        }

        protected virtual void OnDestroy()
        {
            _customButton.OnDoubleClick -= OnDoubleClicked;
        }

        protected virtual void OnValidate()
        {
            if (_customButton == null)
            {
                _customButton = GetComponent<CustomUIButton>();
            }
        }

        private void OnDoubleClicked()
        {
            OnExecute();
        }

        public virtual void Initialize(OS os)
        {
            _os = os;
        }

        public virtual void OnExecute()
        {
            if (_os == null)
            {
                Debug.LogError($"{name}: no OS found to Execute the app.");
                return;
            }

            _currentInstance = Instantiate(_prefab, _runningAppsParent);
            _currentInstance.Initialize(_os);
        }

        public virtual Vector3 GetRandomOffsetWindow()
        {
            Vector3 offset = new Vector3(Random.Range(-200, 200), Random.Range(-150, 0), 0f);
            return offset;
        }
    }
}

