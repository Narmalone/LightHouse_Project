using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LightHouse.Game.UI; // pour UILineRendererWithoutGrid

namespace LightHouse.Game.Computer.LEO.Weather.Humidity
{
    /// <summary>
    /// Contrôleur d'humidité :
    /// - Lit un slider radial (0..1)
    /// - Met à jour l'aiguille + le texte
    /// - Enregistre l'historique dans un graphe (UILineRendererWithoutGrid)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UI_HumidityRateController : MonoBehaviour
    {
        #region Serialized Fields — Wiring & Look

        [Header("Gauge")]
        [SerializeField] private RectTransform _needleTransform;      // pivot de l'aiguille
        [SerializeField] private float _needleRotationOffset = -180f; // selon l'orientation du sprite
        [SerializeField] private TMP_Text _valueText;

        [Header("Input (Radial Slider)")]
        [SerializeField] private ClickableCircleSlider _radialSlider; // émet des valeurs 0..1

        [Header("History Graph")]
        [SerializeField] private UILineRendererWithoutGrid _lineRenderer;
        [SerializeField] private int _maxHistoryPoints = 8;
        [SerializeField] private float _xSpacing = 65f;

        #endregion

        #region State & Events

        /// <summary>Valeur d’humidité normalisée (0..1).</summary>
        public float CurrentHumidity01 { get; private set; }

        /// <summary>Valeur d’humidité en pourcentage (0..100).</summary>
        public float CurrentHumidityPercent => CurrentHumidity01 * 100f;

        /// <summary>Émis quand l’humidité change (0..1).</summary>
        public event Action<float> OnHumidityChanged01;

        /// <summary>Émis quand l’humidité change (0..100).</summary>
        public event Action<float> OnHumidityChangedPercent;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_radialSlider != null)
                _radialSlider.OnValueChanged += OnRadialSliderValueChanged;
        }

        private void Start()
        {
            // Valeur par défaut lisible (15%) au démarrage.
            SetHumidityPercent(15f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SendDatasToGraph(UnityEngine.Random.Range(0, 100f));
            }
        }

        private void OnDestroy()
        {
            if (_radialSlider != null)
                _radialSlider.OnValueChanged -= OnRadialSliderValueChanged;
        }

        private void OnValidate()
        {
            if (_needleTransform == null)
                _needleTransform = GetComponentInChildren<RectTransform>(true);

            if (_valueText == null)
                _valueText = GetComponentInChildren<TMP_Text>(true);

            if (_maxHistoryPoints < 1) _maxHistoryPoints = 1;
            if (_xSpacing <= 0f) _xSpacing = 1f;
        }

        #endregion

        #region Input Handlers

        /// <summary>Reçoit la valeur normalisée [0..1] du slider radial.</summary>
        private void OnRadialSliderValueChanged(float normalizedValue)
        {
            SetHumidity01(normalizedValue);
        }

        #endregion

        #region Public API

        /// <summary>Définit l’humidité via une valeur normalisée [0..1].</summary>
        public void SetHumidity01(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (Mathf.Approximately(normalized, CurrentHumidity01)) return;

            CurrentHumidity01 = normalized;
            ApplyHumidityToUI();

            OnHumidityChanged01?.Invoke(CurrentHumidity01);
            OnHumidityChangedPercent?.Invoke(CurrentHumidityPercent);
        }

        /// <summary>Définit l’humidité via un pourcentage [0..100].</summary>
        public void SetHumidityPercent(float percent)
        {
            SetHumidity01(Mathf.Clamp01(percent / 100f));
        }

        /// <summary>
        /// Ajoute un point au graphe (valeur en pourcentage 0..100).
        /// </summary>
        public void SendDatasToGraph(float humidityPercent)
        {
            if (_lineRenderer == null) return;

            // 1) Normalise le pourcentage → 0..1
            float t01 = Mathf.Clamp01(humidityPercent / 100f);

            // 2) Convertit en coordonnées locales du graphe
            RectTransform rt = _lineRenderer.rectTransform;
            float y = t01 * rt.rect.height;

            // X sera recalculé ensuite via RecalculateX, on met un placeholder ici
            //AddPointAndRespace(new Vector2(0f, y));
            AppendPointWithIndexSpacing(y);
        }

        // --- Remplace RecalculateX ---
        public void RecalculateX(float spacing)
        {
            if (_lineRenderer == null) return;

            var src = _lineRenderer.Points;          // IReadOnlyList<Vector2>
            var dst = new List<Vector2>(src.Count);  // on reconstruit proprement

            for (int i = 0; i < src.Count; i++)
                dst.Add(new Vector2(i * spacing, src[i].y));

            _lineRenderer.SetPoints(dst);
        }

        // --- Remplace AddPointAndRespace (et renomme-le si tu veux) ---
        private void AppendPointWithIndexSpacing(float y)
        {
            if (_lineRenderer == null) return;

            var src = _lineRenderer.Points;
            int capacity = _maxHistoryPoints;

            // 1) Construit la nouvelle liste en droppant le plus vieux si plein
            var dst = new List<Vector2>(Mathf.Min(src.Count + 1, capacity));
            int start = Mathf.Max(0, src.Count - (capacity - 1)); // garde les (capacity-1) plus récents
            for (int i = start; i < src.Count; i++)
                dst.Add(src[i]);

            // 2) Ajoute le nouveau point à l'index suivant
            int newIndex = dst.Count;                // 0..N-1 -> prochain index
            float x = newIndex * _xSpacing;
            dst.Add(new Vector2(x, y));

            // 3) Assure l’indexation régulière 0..N-1 (utile quand on a droppé le plus vieux)
            for (int i = 0; i < dst.Count; i++)
                dst[i] = new Vector2(i * _xSpacing, dst[i].y);

            // 4) Push au renderer
            _lineRenderer.SetPoints(dst);
        }

        #endregion

        #region Internals

        private void ApplyHumidityToUI()
        {
            // Label "xx %"
            if (_valueText != null)
                _valueText.text = Mathf.RoundToInt(CurrentHumidityPercent) + " %";

            // Aiguille : orientée vers le point extérieur cliqué du slider
            if (_needleTransform != null && _radialSlider != null)
            {
                Vector3 pivot = _needleTransform.position;
                Vector3 target = _radialSlider.WorldOuterPoint;

                Vector3 dir = target - pivot;
                float angleZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _needleTransform.localEulerAngles = new Vector3(0f, 0f, angleZ + _needleRotationOffset);
            }
        }

        #endregion
    }
}
