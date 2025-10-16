using LightHouse.Game.Computer.Calendar;
using LightHouse.Handlers;
using LightHouse.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightHouse.Game.Computer.OS
{
    /// <summary>
    /// Gère le système d'exploitation in-game : ouverture des apps, gestion des raccourcis, etc.
    /// </summary>
    public class OS : TabCanvas
    {
        #region Fields & Properties

        [Header("Services Reference")]
        private ComputerServices _services;

        [Header("App References")]
        [Tooltip("Liste des instances d'applications en cours.")]
        public List<ComputerApp> apps = new List<ComputerApp>();

        [Tooltip("Liste des raccourcis associés aux applications.")]
        public List<ShortCutController> ShortCuts { get; set; } = new List<ShortCutController>();

        [SerializeField]
        [Tooltip("Parent transform pour héberger les applications instanciées.")]
        private RectTransform _runningAppsParent;

        [SerializeField]
        private ShortcutButtonsManager _shortcutButtonsManager;

        [SerializeField] private CalendarEventDatabase _eventDatabase;
        [SerializeField] private CanvasGroup _desktopGroup;
        [SerializeField] private OsBoot _bootSystem;
        [SerializeField] private AudioCue _osLoopSound;
        /// <summary>
        /// Le parent dans la hiérarchie pour les applications ouvertes.
        /// </summary>
        public RectTransform RunningAppParent => _runningAppsParent;

        public ShortcutButtonsManager ShortcutButtonsManager => _shortcutButtonsManager;

        /// <summary>
        /// Dictionnaire des apps ouvertes indexées par nom.
        /// </summary>
        private Dictionary<string, ComputerApp> _openedApps = new();

        public bool PlayerOnComputer { get; set; } = false;
        public ComputerServices Services => _services;

        public event Action OnLeftComputerCalled;

        [SerializeField] private AudioCue _clickSoundEffect;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ShortCuts = GetComponentsInChildren<ShortCutController>().ToList();
            _eventDatabase.ClearRuntimeEvents();
            // Initialise chaque raccourci avec cette instance d'OS
            foreach (var shortcut in ShortCuts)
            {
                shortcut.Initialize(this);
            }
            InputManager.UI.Click.performed += Click_performed;
            InputManager.OnInputManagerWillClear += OnInputManagerCleared;
        }

        private void Start()
        {
            _desktopGroup.alpha = 0.0f;
            _desktopGroup.interactable = false;
            _desktopGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            _eventDatabase.ClearRuntimeEvents();
        }

        private void OnInputManagerCleared()
        {
            InputManager.UI.Click.performed -= Click_performed;
        }

        private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ServiceLocator.Audio != null && _clickSoundEffect != null && 
                PlayerHandlerData.MainPlayer != null && 
                PlayerHandlerData.MainPlayer.PlayerState == KinematicCharacterController.PlayerState.ComputerMode)
                ServiceLocator.Audio.PlayAt(_clickSoundEffect, transform.position);
        }

        public void SetService(ComputerServices services)
        {
            this._services = services;
        }

        private IAudioHandle _loopOs;
        public void BootOS()
        {
            _bootSystem.StartBoot(() =>
            {
                _desktopGroup.alpha = 1.0f;
                _desktopGroup.interactable = true;
                _desktopGroup.blocksRaycasts = true;

                if(ServiceLocator.Audio != null && _osLoopSound)
                {
                    _loopOs = ServiceLocator.Audio.PlayAt(_osLoopSound, this.transform.position);
                }
            });
        }

        public void LeaveOS()
        {
            if(_loopOs != null)
            {
                _loopOs.Stop(1f);
                _loopOs = null;
            }
            OnLeftComputerCalled?.Invoke();
            ShortcutButtonsManager.SwitchSelectedButton(null);
            _desktopGroup.alpha = 0.0f;
            _desktopGroup.interactable = false;
            _desktopGroup.blocksRaycasts = false;
        }

        #endregion
    }
}
