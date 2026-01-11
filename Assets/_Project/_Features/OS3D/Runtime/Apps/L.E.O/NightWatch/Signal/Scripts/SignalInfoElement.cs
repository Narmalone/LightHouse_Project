using LightHouse.Features.Signals;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.LEO.NightWatch.Signals
{
    /// <summary>
    /// Élément UI affichant un signal actif (icône, label, timer).
    /// </summary>
    public class SignalInfoElement : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private TextMeshProUGUI _timerText;

        #endregion

        #region Private Fields

        private ISignal _model;

        #endregion

        #region Properties

        /// <summary>Modèle de données associé à cet élément UI.</summary>
        public ISignal Model => _model;

        /// <summary>Clé unique du signal (souvent utilisée pour l'identification rapide).</summary>
        public string Key => _model?.Key;

        /// <summary>Accès direct à l'icône de l'élément.</summary>
        public Image Icon => _icon;

        #endregion

        #region Events

        /// <summary>
        /// Émis lorsque le timer atteint zéro ou moins.
        /// </summary>
        public event Action<ISignal> OnTimerEnded;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise l'élément avec un modèle et son icône.
        /// </summary>
        public void Initialize(ISignal model, Sprite icon)
        {
            if (model == null)
            {
                Debug.LogWarning("[SignalInfoElement] Initialize appelé avec un modèle nul.");
                return;
            }

            _model = model;
            _icon.sprite = icon;
            _label.text = model.DisplayText;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (_model == null)
                return;

            // Vérifie expiration
            if (_model.RemainingTime <= 0f)
            {
                OnTimerEnded?.Invoke(_model);
                return;
            }

            // Met à jour l'affichage du temps restant
            UpdateTimerDisplay(_model.RemainingTime);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Formatte et applique le temps restant dans le label timer.
        /// </summary>
        private void UpdateTimerDisplay(float remainingSeconds)
        {
            int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        #endregion
    }
}
