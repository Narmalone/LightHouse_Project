using UnityEngine;

namespace LightHouse.Game.Computer.OS
{
    /// <summary>
    /// Contrôleur générique pour les raccourcis d'application sur le bureau.
    /// Gère l'exécution d'une application via double-clic, avec support pour l'instanciation ou la réactivation.
    /// </summary>
    public abstract class ShortCutController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("App Settings")]
        [SerializeField] private ComputerApp _prefab;
        [SerializeField] private bool _allowMultipleInstance = true;

        [Header("UI Elements")]
        [SerializeField] private CustomUIButton _customButton;

        #endregion

        #region Runtime References

        protected OS _os;
        protected ComputerApp _currentInstance;
        private RectTransform _runningAppsParent => _os.RunningAppParent;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_customButton != null)
                _customButton.OnDoubleClick += OnDoubleClicked;
        }

        protected virtual void OnDestroy()
        {
            if (_customButton != null)
                _customButton.OnDoubleClick -= OnDoubleClicked;
        }

        protected virtual void OnValidate()
        {
            if (_customButton == null)
                _customButton = GetComponent<CustomUIButton>();
        }

        #endregion

        #region Core Logic

        private void OnDoubleClicked()
        {
            OnExecute();
        }

        public virtual void Initialize(OS os)
        {
            _os = os;
        }

        /// <summary>
        /// Called when we want to launch the app associated with this shortcut
        /// <see cref="ComputerApp"/>
        /// </summary>
        public virtual void OnExecute()
        {
            if (_os == null)
            {
                Debug.LogError($"{name}: no OS found to Execute the app.");
                return;
            }

            if (_currentInstance != null)
            {
                if (_prefab.OpenMode == AppOpenMode.ReactivateIfExists)
                {
                    _currentInstance.gameObject.SetActive(true);
                    _currentInstance.OnOpen();
                    return;
                }
                else if (!_allowMultipleInstance)
                {
                    Destroy(_currentInstance.gameObject);
                }
            }

            _currentInstance = Instantiate(_prefab, _runningAppsParent);
            _currentInstance.Initialize(_os);
            _currentInstance.OnOpen();
        }

        public virtual Vector3 GetRandomOffsetWindow()
        {
            return new Vector3(Random.Range(-200, 200), Random.Range(-150, 0), 0f);
        }

        #endregion
    }
}
