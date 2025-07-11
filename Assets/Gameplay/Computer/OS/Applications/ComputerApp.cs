using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.OS
{
    /// <summary>
    /// Mode d’ouverture d’une application :
    /// - Instancier une nouvelle instance
    /// - Réactiver une instance déjà existante
    /// </summary>
    public enum AppOpenMode
    {
        InstantiateNew,
        ReactivateIfExists
    }

    /// <summary>
    /// États possibles d'une application.
    /// </summary>
    public enum E_ComputerAppState
    {
        None,
        Opened,
        Closed,
        Minimized
    }

    /// <summary>
    /// Classe de base abstraite pour les applications sur l'OS.
    /// Gère l’état, l’ouverture/fermeture/minimisation et l'intégration avec l'OS.
    /// </summary>
    public abstract class ComputerApp : MonoBehaviour
    {
        #region Serialized Fields

        [Header("App Settings")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _rectTransform;

        [field: SerializeField] public string AppName { get; protected set; }
        [field: SerializeField] public AppOpenMode OpenMode { get; private set; } = AppOpenMode.InstantiateNew;

        #endregion

        #region Properties & State

        public E_ComputerAppState State;
        public bool IsMinimized { get; private set; }
        public RectTransform RectTransform => _rectTransform;

        protected OS _os;

        public OS OS => _os;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnClose);
        }

        private void OnEnable()
        {
            State = E_ComputerAppState.Opened;
        }

        private void OnDisable()
        {
            State = E_ComputerAppState.Closed;
        }

        protected virtual void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnClose);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialise l'application avec une référence à l'OS.
        /// </summary>
        public virtual void Initialize(OS os)
        {
            _os = os;
        }

        /// <summary>
        /// Change l’état de l’application entre réduit et restauré.
        /// </summary>
        public void ToggleMinimize()
        {
            IsMinimized = !IsMinimized;
            OnMinimize();
        }

        #endregion

        #region Abstract Methods

        public abstract void OnOpen();
        public abstract void OnClose();
        public abstract void OnMinimize();

        #endregion
    }
}
