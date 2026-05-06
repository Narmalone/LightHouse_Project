using UnityEngine;
using System;
using TMPro;

namespace LightHouse.Features.Menu.Radio
{
    /// <summary>
    /// Gère la fréquence radio, son affichage et l'état du signal.
    /// </summary>
    public class RadioFrequencyController : MonoBehaviour
    {
        #region ===== Dependencies =====

        [SerializeField] private TextMeshProUGUI _frequencyDisplay;

        #endregion

        #region ===== Settings =====

        [SerializeField] private float _targetFrequency = 92.5f;
        [SerializeField] private float _tolerance = 0.3f;

        #endregion

        #region ===== State =====

        public float CurrentFrequency { get; private set; }

        private bool _isInRange;

        #endregion

        #region ===== Events =====

        public event Action<float, float> OnFrequencyChanged;
        public event Action<bool> OnSignalStateChanged;

        #endregion

        #region ===== Public API =====

        public void SetFrequency(float value)
        {
            CurrentFrequency = value;

            UpdateDisplay();
            NotifyFrequencyChanged();
            UpdateSignalState();
        }

        public void ShowFrequencyText()
        {
            SetDisplayVisible(true);
        }

        public void HideFrequencyText()
        {
            SetDisplayVisible(false);
        }

        #endregion

        #region ===== Display =====

        private void UpdateDisplay()
        {
            if (_frequencyDisplay == null) return;

            _frequencyDisplay.text = $"{CurrentFrequency:0.0} FM";
        }

        private void SetDisplayVisible(bool visible)
        {
            if (_frequencyDisplay == null) return;

            _frequencyDisplay.gameObject.SetActive(visible);
        }

        #endregion

        #region ===== Logic =====

        private void NotifyFrequencyChanged()
        {
            OnFrequencyChanged?.Invoke(CurrentFrequency, _targetFrequency);
        }

        private void UpdateSignalState()
        {
            bool inRange = IsInRange(CurrentFrequency);

            if (inRange == _isInRange) return;

            _isInRange = inRange;
            OnSignalStateChanged?.Invoke(_isInRange);
        }

        private bool IsInRange(float frequency)
        {
            return Mathf.Abs(frequency - _targetFrequency) <= _tolerance;
        }

        #endregion
    }
}