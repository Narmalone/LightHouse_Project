using System;
using TMPro;
using UnityEngine;
using LightHouse.Weather;

namespace LightHouse.Game.Computer.LEO.Weather.Wind
{
    /// <summary>
    /// Élément de la boussole : un bouton + un label cardinal.
    /// Émet un événement quand l'utilisateur clique dessus.
    /// Gère aussi l'état visuel (sélection/désélection).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CompassArrowElement : MonoBehaviour
    {
        #region Serialized Fields — Wiring

        [Header("Config")]
        [SerializeField] private WindOrientationType _windOrientation;

        [Header("UI")]
        [SerializeField] private UI_CustomButton _button;
        [SerializeField] private TextMeshProUGUI _cardinalText;

        #endregion

        #region Events & Public API

        /// <summary>
        /// Événement levé quand cet élément est cliqué.
        /// </summary>
        public event Action<CompassArrowElement> CompassArrow;

        /// <summary>Orientation associée à cet élément.</summary>
        public WindOrientationType WindOrientation => _windOrientation;

        /// <summary>Bouton sous-jacent (accès lecture seule).</summary>
        public UI_CustomButton Button => _button;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_button != null)
                _button.OnClick += OnClicked;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.OnClick -= OnClicked;
        }

        private void OnValidate()
        {
            // Auto-récupération si omis dans l’inspecteur
            if (_button == null)
                _button = GetComponentInChildren<UI_CustomButton>(true);

            if (_cardinalText == null)
                _cardinalText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        #endregion

        #region Handlers

        private void OnClicked(UI_CustomButton _)
        {
            CompassArrow?.Invoke(this);
        }

        #endregion

        #region Visual State

        /// <summary>
        /// Applique l'état "sélectionné" (couleur + visuel bouton).
        /// </summary>
        public void OnSelect()
        {
            if (_button != null)
                _button.Select();

            if (_cardinalText != null)
                _cardinalText.color = (_button != null) ? _button.selectedColor : Color.white;
        }

        /// <summary>
        /// Applique l'état "désélectionné".
        /// </summary>
        public void OnDeselect()
        {
            if (_button != null)
                _button.Deselect();

            if (_cardinalText != null)
                _cardinalText.color = Color.white;
        }

        #endregion
    }
}
