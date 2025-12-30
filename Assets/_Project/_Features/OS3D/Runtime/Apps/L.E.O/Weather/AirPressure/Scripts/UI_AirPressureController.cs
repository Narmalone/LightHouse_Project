using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Weather.Pressure
{
    #region Data

    /// <summary>
    /// Plage de pression affichée/sélectionnée dans l'UI (en hPa).
    /// </summary>
    [Serializable]
    public struct PressureRange
    {
        public float MinPressure;
        public float MaxPressure;

        public PressureRange(float min, float max)
        {
            MinPressure = min;
            MaxPressure = max;
        }

        public readonly float Midpoint => (MinPressure + MaxPressure) * 0.5f;

        public readonly bool Contains(float hPa) =>
            hPa >= MinPressure && hPa <= MaxPressure;

        public override readonly string ToString() =>
            $"{MinPressure:0.#}-{MaxPressure:0.#} hPa";
    }

    #endregion

    /// <summary>
    /// Contrôleur UI pour la sélection d'une plage de pression atmosphérique via des boutons.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UI_AirPressureController : MonoBehaviour
    {
        #region Serialized Fields — Wiring & Look

        [Header("Buttons")]
        [SerializeField] private PressureButton[] _pressureButtons;

        [Header("UI Output")]
        [SerializeField] private TextMeshProUGUI _selectedHpaText;

        [Header("Colors")]
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _inactiveColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        [Header("Init")]
        [Tooltip("Sélection auto du premier bouton au Start.")]
        [SerializeField] private bool _autoSelectFirstOnStart = true;

        #endregion

        #region State

        /// <summary>Plage actuellement sélectionnée.</summary>
        public PressureRange CurrentRange { get; private set; } = new PressureRange(0, 0);

        /// <summary>Index du bouton sélectionné, -1 si aucun.</summary>
        public int CurrentIndex { get; private set; } = -1;

        /// <summary>Événement levé quand la sélection change.</summary>
        public event Action<PressureRange> OnPressureRangeChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Abonne chaque bouton
            foreach (var btn in _pressureButtons)
                btn.OnClick += OnPressureButtonClicked;
        }

        private void Start()
        {
            if (_autoSelectFirstOnStart && _pressureButtons != null && _pressureButtons.Length > 0)
                SelectByIndex(0);
            else
                RefreshLabel();
        }

        private void OnDestroy()
        {
            foreach (var btn in _pressureButtons)
                btn.OnClick -= OnPressureButtonClicked;
        }

        private void OnValidate()
        {
            // Remonte automatiquement les enfants si non renseignés
            if (_pressureButtons == null || _pressureButtons.Length == 0)
                _pressureButtons = GetComponentsInChildren<PressureButton>(true)
                    .OrderByDescending(p => p.transform.GetSiblingIndex())
                    .ToArray();
        }

        #endregion

        #region Event Handlers

        private void OnPressureButtonClicked(PressureButton clicked)
        {
            int idx = Array.IndexOf(_pressureButtons, clicked);
            if (idx >= 0)
                SelectByIndex(idx);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sélectionne un bouton par index (0..N-1). Ignore si hors bornes.
        /// </summary>
        public void SelectByIndex(int index)
        {
            if (_pressureButtons == null || _pressureButtons.Length == 0) return;
            if (index < 0 || index >= _pressureButtons.Length) return;

            var selected = _pressureButtons[index];
            CurrentIndex = index;
            CurrentRange = new PressureRange(selected.MinAirPressure, selected.MaxAirPressure);

            UpdateButtonsVisual();
            RefreshLabel();

            OnPressureRangeChanged?.Invoke(CurrentRange);
        }

        /// <summary>
        /// Sélectionne la plage qui contient la pression donnée (si existante).
        /// </summary>
        public void SelectByPressure(float hPa)
        {
            if (_pressureButtons == null || _pressureButtons.Length == 0) return;

            for (int i = 0; i < _pressureButtons.Length; i++)
            {
                var b = _pressureButtons[i];
                if (hPa >= b.MinAirPressure && hPa <= b.MaxAirPressure)
                {
                    SelectByIndex(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Raccourci pour fixer l'index via code (ex: binding depuis un dropdown).
        /// </summary>
        public void SetAirPressure(int targetIndex) => SelectByIndex(targetIndex);

        #endregion

        #region UI Helpers

        /// <summary>
        /// Met à jour la couleur des boutons : allume le sélectionné
        /// ET tous ceux en dessous (index plus petits), car l'ordre est inversé.
        /// </summary>
        private void UpdateButtonsVisual()
        {
            if (_pressureButtons == null) return;

            for (int i = 0; i < _pressureButtons.Length; i++)
            {
                var graphic = _pressureButtons[i].Button?.targetGraphic;
                if (graphic == null) continue;

                // Avant : i >= CurrentIndex (du sélectionné vers la fin)
                // Maintenant : i <= CurrentIndex (du sélectionné vers le début)
                bool isActive = (CurrentIndex >= 0) && (i <= CurrentIndex);
                graphic.color = isActive ? _activeColor : _inactiveColor;
            }
        }

        /// <summary>
        /// Mets à jour le label texte avec la plage courante.
        /// </summary>
        private void RefreshLabel()
        {
            if (_selectedHpaText == null) return;

            if (CurrentIndex >= 0)
                _selectedHpaText.text = $"{CurrentRange.MinPressure:0.#}-{CurrentRange.MaxPressure:0.#} hPa";
            else
                _selectedHpaText.text = "—";
        }

        #endregion
    }
}
