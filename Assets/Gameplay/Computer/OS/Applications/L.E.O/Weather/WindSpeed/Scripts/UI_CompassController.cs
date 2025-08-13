using System;
using UnityEngine;
using LightHouse.Weather;

namespace LightHouse.Game.Computer.LEO.Weather.Wind
{
    /// <summary>
    /// Gère la sélection de la direction du vent via une "boussole" de boutons.
    /// - Un seul bouton sélectionné à la fois
    /// - Expose l’orientation et l’index courants
    /// - Permet la sélection par direction (API)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UI_CompassController : MonoBehaviour
    {
        #region Serialized Fields — Wiring

        [Header("Compass Buttons (children)")]
        [Tooltip("Boutons de la boussole dans l’ordre souhaité (ex: N, NE, E, …). " +
                 "S’ils ne sont pas renseignés, ils seront auto-récupérés parmi les enfants.")]
        [SerializeField] private CompassArrowElement[] _buttons;

        [Header("Init")]
        [Tooltip("Sélectionner automatiquement le premier bouton au Start.")]
        [SerializeField] private bool _autoSelectFirstOnStart = true;

        #endregion

        #region State & Events

        private CompassArrowElement _lastSelected;

        /// <summary>Index du bouton sélectionné, -1 si aucun.</summary>
        public int CurrentSelectedIndex =>
            _lastSelected != null ? Array.IndexOf(_buttons, _lastSelected) : -1;

        /// <summary>Orientation actuellement sélectionnée.</summary>
        public WindOrientationType CurrentSelectedOrientation =>
            _lastSelected != null ? _lastSelected.WindOrientation : default;

        /// <summary>Émis quand l’orientation change (suite à une sélection utilisateur ou code).</summary>
        public event Action<WindOrientationType> OnOrientationChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Récupère les enfants si la liste n’a pas été assignée manuellement
            if (_buttons == null || _buttons.Length == 0)
                _buttons = GetComponentsInChildren<CompassArrowElement>(true);

            // Abonne les clics
            foreach (var b in _buttons)
                b.CompassArrow += OnButtonClicked;
        }

        private void Start()
        {
            if (_autoSelectFirstOnStart && _buttons != null && _buttons.Length > 0)
                SetSelected(_buttons[0]);
        }

        private void OnDestroy()
        {
            foreach (var b in _buttons)
                b.CompassArrow -= OnButtonClicked;
        }

        private void OnValidate()
        {
            if (_buttons == null || _buttons.Length == 0)
                _buttons = GetComponentsInChildren<CompassArrowElement>(true);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sélectionne le bouton correspondant à l’orientation demandée.
        /// </summary>
        public void SelectByDirection(WindOrientationType direction)
        {
            if (_buttons == null || _buttons.Length == 0) return;

            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i].WindOrientation == direction)
                {
                    SetSelected(_buttons[i]);
                    return;
                }
            }

            Debug.LogWarning($"[WindButtonController] Aucun bouton trouvé pour l’orientation {direction}.");
        }

        /// <summary>
        /// Désélectionne tout (aucune orientation active).
        /// </summary>
        public void ClearSelection()
        {
            if (_lastSelected != null)
            {
                _lastSelected.OnDeselect();
                _lastSelected = null;
                OnOrientationChanged?.Invoke(default);
            }
        }

        #endregion

        #region Internals

        private void OnButtonClicked(CompassArrowElement clicked)
        {
            SetSelected(clicked);
        }

        /// <summary>
        /// Applique visuellement et logiquement la sélection d’un bouton donné.
        /// </summary>
        private void SetSelected(CompassArrowElement newButton)
        {
            if (newButton == null) return;
            if (_lastSelected == newButton) return; // déjà sélectionné

            // Deselect ancien
            if (_lastSelected != null)
                _lastSelected.OnDeselect();

            // Select nouveau
            _lastSelected = newButton;
            _lastSelected.OnSelect();

            // Notifie l’orientation actuelle
            OnOrientationChanged?.Invoke(_lastSelected.WindOrientation);
        }

        #endregion
    }
}
