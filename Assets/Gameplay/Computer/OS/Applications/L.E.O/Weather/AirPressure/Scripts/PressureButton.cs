using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Weather
{
    /// <summary>
    /// Bouton de sélection d'une plage de pression atmosphérique (en hPa).
    /// Relaye le clic via l'événement <see cref="OnClick"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class PressureButton : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Wiring")]
        [SerializeField] private Button _button;

        [Header("Pressure Range (hPa)")]
        [SerializeField] private float _minAirPressure = 980f;
        [SerializeField] private float _maxAirPressure = 1010f;

        #endregion

        #region Events

        /// <summary>Émis quand l'utilisateur clique ce bouton.</summary>
        public event Action<PressureButton> OnClick;

        #endregion

        #region Public API

        public Button Button => _button;
        public float MinAirPressure => _minAirPressure;
        public float MaxAirPressure => _maxAirPressure;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Récupération de la référence si oubliée dans l'inspecteur
            if (_button == null)
                _button = GetComponent<Button>();

            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnValidate()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            // Sécurité : si bornes inversées on corrige
            if (_maxAirPressure < _minAirPressure)
            {
                float tmp = _minAirPressure;
                _minAirPressure = _maxAirPressure;
                _maxAirPressure = tmp;
            }
        }

        #endregion

        #region Handlers

        private void OnButtonClicked()
        {
            OnClick?.Invoke(this);
        }

        #endregion
    }
}
