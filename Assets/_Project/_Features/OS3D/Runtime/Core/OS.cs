using LightHouse.Features.Computer.LEO;
using LightHouse.Core.Audio;
using LightHouse.Features.Computer.Calendar;
using LightHouse.Core.Inputs;
using LightHouse.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightHouse.Core.Settings;

public enum OSInteractionContext
{
    None,
    Computer,
    Locked // options, popup etc
}

namespace LightHouse.Features.Computer.OS
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
        [SerializeField] private CanvasGroup _bootGroup;
        [SerializeField] private BootController _bootSystem;
        [SerializeField] private SO_AudioCue _osLoopSound;
        [SerializeField] private SO_AudioCue _startOsSound;
        /// <summary>
        /// Le parent dans la hiérarchie pour les applications ouvertes.
        /// </summary>
        public RectTransform RunningAppParent => _runningAppsParent;

        public ShortcutButtonsManager ShortcutButtonsManager => _shortcutButtonsManager;

        /// <summary>
        /// Dictionnaire des apps ouvertes indexées par nom.
        /// </summary>
        private Dictionary<string, ComputerApp> _openedApps = new();
        private IAudioHandle _startOsSoundAudio;
        private IAudioHandle _loopOs;

        public bool PlayerOnComputer { get; set; } = false;
        public ComputerServices Services => _services;

        public event Action OnLeftComputerCalled;

        [SerializeField] private SO_AudioCue _clickSoundEffect;
        public OSInteractionContext CurrentContext { get; private set; } = OSInteractionContext.None;

        [field: SerializeField] public bool SkipBootSequence { get; set; } = false;

        #endregion

        public void SetContext(OSInteractionContext context)
        {
            CurrentContext = context;
        }

        private void SettingsMenuController_OnSettingsMenuClosed()
        {
            if (!PlayerOnComputer) return;
            this.SetContext(OSInteractionContext.Computer);
        }

        private void SettingsMenuController_OnSettingsMenuOpened()
        {
            if (!PlayerOnComputer) return;
            this.SetContext(OSInteractionContext.Locked);
        }

        #region Unity Lifecycle

        private void Awake()
        {
            ShortCuts = GetComponentsInChildren<ShortCutController>().ToList();
            _eventDatabase.ClearRuntimeEvents();

            SettingsMenuController.OnSettingsMenuOpened += SettingsMenuController_OnSettingsMenuOpened;
            SettingsMenuController.OnSettingsMenuClosed += SettingsMenuController_OnSettingsMenuClosed;

            // Initialise chaque raccourci avec cette instance d'OS
            foreach (var shortcut in ShortCuts)
            {
                shortcut.Initialize(this);
            }
        }

        private void OnEnable()
        {
            InputManager.UI.Click.performed += Click_performed;
        }

        private void OnDisable()
        {
            InputManager.UI.Click.performed -= Click_performed;
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
            SettingsMenuController.OnSettingsMenuOpened -= SettingsMenuController_OnSettingsMenuOpened;
            SettingsMenuController.OnSettingsMenuClosed -= SettingsMenuController_OnSettingsMenuClosed;
        }

        private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (ServiceLocator.Audio != null && _clickSoundEffect != null)
            {
                if (CurrentContext == OSInteractionContext.Computer)
                {
                    ServiceLocator.Audio.PlayAt(_clickSoundEffect, transform.position);
                }
            }
        }

        public void SetService(ComputerServices services)
        {
            this._services = services;
        }

        public void BootOS()
        {
            this.SetContext(OSInteractionContext.Computer);
            _bootGroup.alpha = 1.0f;
            PlayerOnComputer = true;

            if (ServiceLocator.Audio != null && _startOsSound)
            {
                _startOsSoundAudio = ServiceLocator.Audio.PlayAt(_startOsSound, this.transform.position);
            }

            StartCoroutine(StartOsLoopSound());

            if (SkipBootSequence)
            {
                _desktopGroup.alpha = 1.0f;
                _desktopGroup.interactable = true;
                _desktopGroup.blocksRaycasts = true;

                _bootGroup.alpha = 0.0f;
                _bootGroup.interactable = false;
                _bootGroup.blocksRaycasts = false;
                return;
            }

            _bootSystem.StartBoot(() =>
            {
                _desktopGroup.alpha = 1.0f;
                _desktopGroup.interactable = true;
                _desktopGroup.blocksRaycasts = true;

                _bootGroup.alpha = 0.0f;
                _bootGroup.interactable = false;
                _bootGroup.blocksRaycasts = false;  
            });

        }

        private System.Collections.IEnumerator StartOsLoopSound()
        {
            yield return new WaitForSeconds(_startOsSoundAudio.SelectedClip.length);

            if(_startOsSound != null)
                _startOsSound = null;

            if (ServiceLocator.Audio != null && _osLoopSound)
            {
                _loopOs = ServiceLocator.Audio.PlayAt(_osLoopSound, this.transform.position);
            }
        }

        public void LeaveOS()
        {
            this.SetContext(OSInteractionContext.None);
            _bootSystem.StopBoot();
            if (_loopOs != null)
            {
                _loopOs.Stop(1f);
                _loopOs = null;
            }

            OnLeftComputerCalled?.Invoke();
            ShortcutButtonsManager.ForceUnselect();
            _desktopGroup.alpha = 0.0f;
            _desktopGroup.interactable = false;
            _desktopGroup.blocksRaycasts = false;

            PlayerOnComputer = false;
        }

        #endregion
    }
}
