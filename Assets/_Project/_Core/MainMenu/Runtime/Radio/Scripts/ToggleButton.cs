using System;
using UnityEngine;

namespace LightHouse.Core.Interaction
{
    /// <summary>
    /// Bouton toggle avec feedback visuel emissif (hover, actif, inactif).
    /// </summary>
    public class ToggleButton : MonoBehaviour
    {
        #region ===== Events =====

        public event Action<bool> OnValueChanged;

        #endregion

        #region ===== Dependencies =====

        [SerializeField] private Renderer _renderer;

        #endregion

        #region ===== Settings =====

        [Header("Colors")]
        [SerializeField] private Color _enabledColor = Color.green;
        [SerializeField] private Color _hoverColor = Color.orange;
        [SerializeField] private Color _disabledColor = Color.red;

        [Header("Intensity")]
        [SerializeField] private float _enabledIntensity = 3f;
        [SerializeField] private float _hoverIntensity = 3f;
        [SerializeField] private float _disabledIntensity = 3f;

        #endregion

        #region ===== State =====

        private bool _isOn;
        private InteractableBase _interactable;

        private MaterialPropertyBlock _mpb;

        private static readonly int EmissiveColorID = Shader.PropertyToID("_EmissiveColor");

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            _mpb = new MaterialPropertyBlock();
            ApplyCurrentState();
        }

        #endregion

        #region ===== Binding =====

        public void Bind(InteractableBase interactable)
        {
            if (interactable == null) return;

            Unbind(); // 🔥 évite double binding

            _interactable = interactable;

            _interactable.OnHoverEnter += OnHoverEnter;
            _interactable.OnHoverExit += OnHoverExit;
            _interactable.OnClickDown += OnClickDown;
        }

        private void Unbind()
        {
            if (_interactable == null) return;

            _interactable.OnHoverEnter -= OnHoverEnter;
            _interactable.OnHoverExit -= OnHoverExit;
            _interactable.OnClickDown -= OnClickDown;

            _interactable = null;
        }

        #endregion

        #region ===== Interaction =====

        private void OnHoverEnter()
        {
            Apply(_hoverColor, _hoverIntensity);
        }

        private void OnHoverExit()
        {
            ApplyCurrentState();
        }

        private void OnClickDown()
        {
            Toggle();
            ApplyCurrentState();
        }

        #endregion

        #region ===== State Logic =====

        private void Toggle()
        {
            _isOn = !_isOn;
            OnValueChanged?.Invoke(_isOn);
        }

        private void ApplyCurrentState()
        {
            if (_isOn)
                Apply(_enabledColor, _enabledIntensity);
            else
                Apply(_disabledColor, _disabledIntensity);
        }

        #endregion

        #region ===== Visual =====

        private void Apply(Color color, float intensity)
        {
            if (_renderer == null) return;

            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissiveColorID, color * intensity);
            _renderer.SetPropertyBlock(_mpb);
        }

        #endregion
    }
}