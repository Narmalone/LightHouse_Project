using UnityEngine;
using System;
using LightHouse.Core.Interaction;

namespace LightHouse.Features.Menu.Radio
{
    /// <summary>
    /// Gère le dial de fréquence de la radio (rotation + valeur).
    /// </summary>
    public class RadioDial : MonoBehaviour, IClickable, IClickableHold, IClickableUp
    {
        #region ===== Settings =====

        [Header("Value")]
        [SerializeField] private float _sensitivity = 0.2f;
        [SerializeField] private float _minValue = 88f;
        [SerializeField] private float _maxValue = 108f;

        [Header("Rotation")]
        [SerializeField] private float _minAngle = -135f;
        [SerializeField] private float _maxAngle = 135f;

        #endregion

        #region ===== State =====

        public float CurrentValue { get; private set; } = 90f;

        private bool _isDragging;

        #endregion

        #region ===== Events =====

        public event Action<float> OnValueChanged;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Start()
        {
            Initialize();
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            UpdateVisual();
            NotifyValueChanged();
        }

        #endregion

        #region ===== Interaction / Interfaces =====

        public void OnClicked()
        {
            _isDragging = true;
        }

        public void OnClickHold()
        {
            if (!_isDragging) return;

            UpdateValueFromInput();
        }

        public void OnClickReleased()
        {
            _isDragging = false;
        }

        #endregion

        #region ===== Logic =====

        private void UpdateValueFromInput()
        {
            float delta = Input.GetAxis("Mouse X");

            CurrentValue += delta * _sensitivity;
            CurrentValue = Mathf.Clamp(CurrentValue, _minValue, _maxValue);

            NotifyValueChanged();
            UpdateVisual();
        }

        private void NotifyValueChanged()
        {
            OnValueChanged?.Invoke(CurrentValue);
        }

        #endregion

        #region ===== Visual =====

        private void UpdateVisual()
        {
            float normalized = Mathf.InverseLerp(_minValue, _maxValue, CurrentValue);
            float angle = Mathf.Lerp(_minAngle, _maxAngle, normalized);

            transform.localRotation = Quaternion.Euler(angle, -90f, 90f);
        }

        #endregion

        #region ===== Public API =====

        public void ForceUpdateValue()
        {
            NotifyValueChanged();
            UpdateVisual();
        }

        #endregion
    }
}