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
        public List<ShortCutController> ShortCuts = new List<ShortCutController>();

        [SerializeField]
        [Tooltip("Parent transform pour héberger les applications instanciées.")]
        private RectTransform _runningAppsParent;

        /// <summary>
        /// Le parent dans la hiérarchie pour les applications ouvertes.
        /// </summary>
        public RectTransform RunningAppParent => _runningAppsParent;

        /// <summary>
        /// Dictionnaire des apps ouvertes indexées par nom.
        /// </summary>
        private Dictionary<string, ComputerApp> _openedApps = new();

        public bool PlayerOnComputer = false;
        public ComputerServices Services => _services;

        #endregion

        #region Unity Lifecycle

        private void OnValidate()
        {
            // Récupère automatiquement tous les raccourcis enfants
            ShortCuts = GetComponentsInChildren<ShortCutController>().ToList();
        }

        private void Awake()
        {
            // Initialise chaque raccourci avec cette instance d'OS
            foreach (var shortcut in ShortCuts)
            {
                shortcut.Initialize(this);
            }
        }

        public void SetService(ComputerServices services)
        {
            this._services = services;
        }

        #endregion
    }
}
