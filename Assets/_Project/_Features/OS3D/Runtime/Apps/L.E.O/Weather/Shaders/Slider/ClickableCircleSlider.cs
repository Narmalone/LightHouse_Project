using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightHouse.Features.Computer.LEO.Weather.Humidity
{
    /// <summary>
    /// Slider circulaire cliquable/drag (en forme d'anneau, supporte ellipse).
    /// Lit un arc défini dans le shader (Start/End/Direction) et renvoie une valeur normalisée [0..1].
    /// - Clic/drag à l’intérieur de l’anneau : met à jour la valeur
    /// - Expose le point extérieur cliqué (utile pour orienter une aiguille)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ClickableCircleSlider : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Shader Property IDs

        private static readonly int _StartAngleID = Shader.PropertyToID("_StartAngle");
        private static readonly int _EndAngleID = Shader.PropertyToID("_EndAngle");
        private static readonly int _DirectionID = Shader.PropertyToID("_Direction");
        private static readonly int _FillValueID = Shader.PropertyToID("_FillValue");

        #endregion

        #region Serialized Fields — Wiring & Look

        [Header("Geometry")]
        [Tooltip("Épaisseur de l’anneau (0..1), 1 = plein, 0 = aucun.")]
        [Range(0f, 1f)] public float _ringThickness = 0.15f;

        [Tooltip("RectTransform du visuel de l’anneau (ellipse autorisée).")]
        [SerializeField] private RectTransform _ringTransform;

        [Header("Material (Radial Shader)")]
        [Tooltip("Matériau possédant les propriétés _StartAngle, _EndAngle, _Direction, _FillValue.")]
        [SerializeField] private Material _radialMaterial;

        [Header("Debug")]
        [SerializeField] private bool _drawDebugRay = false;

        #endregion

        #region State & Events

        /// <summary>Valeur normalisée [0..1].</summary>
        public float CurrentValue { get; private set; }

        /// <summary>Point local à l’extérieur (sur le bord) de l’ellipse (dans l’espace du rect).</summary>
        public Vector3 OuterLocalPoint { get; private set; }

        /// <summary>Point monde correspondant (utile pour orienter un pointeur).</summary>
        public Vector3 WorldOuterPoint { get; private set; }

        /// <summary>Émis à chaque mise à jour de la valeur (drag/clic).</summary>
        public event Action<float> OnValueChanged;

        /// <summary>Émis quand l’interaction se termine (ex: quitte le slider après un clic/drag).</summary>
        public event Action<float> OnValueCommitted;

        private bool _handlingInteraction;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Fallback si non cablé dans l’inspecteur
            if (_ringTransform == null)
                _ringTransform = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            if (_ringTransform == null)
                _ringTransform = GetComponent<RectTransform>();

            _ringThickness = Mathf.Clamp01(_ringThickness);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Fixe la valeur programmatique (0..1) et met à jour le shader.
        /// </summary>
        public void SetValue01(float t)
        {
            t = Mathf.Clamp01(t);
            CurrentValue = t;
            if (_radialMaterial != null)
                _radialMaterial.SetFloat(_FillValueID, t);
            OnValueChanged?.Invoke(CurrentValue);
        }

        #endregion

        #region EventSystem Handlers

        public void OnPointerClick(PointerEventData eventData) => HandleInteraction(eventData);

        public void OnPointerDown(PointerEventData eventData)
        {
            _handlingInteraction = true;
            HandleInteraction(eventData);
        }

        public void OnDrag(PointerEventData eventData) => HandleInteraction(eventData);

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_handlingInteraction)
            {
                _handlingInteraction = false;
                OnValueCommitted?.Invoke(CurrentValue);
            }
        }

        #endregion

        #region Core

        private void HandleInteraction(PointerEventData eventData)
        {
            if (_ringTransform == null || _radialMaterial == null) return;

            // Convertit le point écran -> local dans l’espace du RectTransform
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _ringTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            // Centre + offset
            Vector2 center = _ringTransform.rect.center;
            Vector2 offset = localPoint - center;

            // Ellipse normalisée (x/rx, y/ry)
            float rx = _ringTransform.rect.width * 0.5f;
            float ry = _ringTransform.rect.height * 0.5f;

            if (rx <= Mathf.Epsilon || ry <= Mathf.Epsilon) return;

            float normX = offset.x / rx;
            float normY = offset.y / ry;

            // Rayon dans l’espace normalisé (ellipse → cercle unité)
            float radius = Mathf.Sqrt(normX * normX + normY * normY);

            // Bords intérieur/extérieur (1 = bord extérieur de l’ellipse)
            float outerRadius = 1f;
            float innerRadius = Mathf.Max(0f, 1f - _ringThickness);

            // En dehors de l’anneau → on ignore
            if (radius < innerRadius || radius > outerRadius)
                return;

            // Angle polaire (0..2π) dans l’espace normalisé
            float angle = Mathf.Atan2(normY, normX);
            if (angle < 0f) angle += Mathf.PI * 2f;

            // Récupère l’arc paramétré dans le shader
            float startAngle = _radialMaterial.GetFloat(_StartAngleID);
            float endAngle = _radialMaterial.GetFloat(_EndAngleID);
            float direction = Mathf.Sign(_radialMaterial.GetFloat(_DirectionID)); // +1/-1

            // Span de l’arc (toujours positif)
            float arcSpan = (direction > 0f)
                ? NormalizeAngle(endAngle - startAngle)
                : NormalizeAngle(startAngle - endAngle);

            // Position cliquée le long de l’arc (0..arcSpan)
            float clickAngle = (direction > 0f)
                ? NormalizeAngle(angle - startAngle)
                : NormalizeAngle(startAngle - angle);

            // En dehors de l’arc visible → ignore
            if (clickAngle > arcSpan)
                return;

            // Nouvelle valeur = progression le long de l’arc
            float newValue = Mathf.Clamp01(clickAngle / Mathf.Max(1e-5f, arcSpan));
            CurrentValue = newValue;

            // Pousse au shader
            _radialMaterial.SetFloat(_FillValueID, newValue);

            // Calcule le point “extérieur” (local & monde) pour faciliter l’orientation d’une aiguille
            Vector2 dir = offset.normalized;
            Vector2 outerLocal = center + dir * (_ringTransform.rect.width * 0.5f);
            OuterLocalPoint = outerLocal;
            WorldOuterPoint = _ringTransform.TransformPoint(outerLocal);

            OnValueChanged?.Invoke(newValue);

            if (_drawDebugRay)
                Debug.DrawRay(WorldOuterPoint, (Vector3)(dir) * 0.5f, Color.yellow, 1.5f);
        }

        /// <summary>Normalise un angle (radians) dans [0, 2π).</summary>
        private static float NormalizeAngle(float angle)
        {
            float twoPI = Mathf.PI * 2f;
            float a = angle % twoPI;
            return (a < 0f) ? a + twoPI : a;
        }

        #endregion

#if UNITY_EDITOR
        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (_ringTransform == null) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
            DrawEllipseGizmo(_ringTransform, 1f);

            Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
            DrawEllipseGizmo(_ringTransform, Mathf.Max(0f, 1f - _ringThickness));
        }

        private static void DrawEllipseGizmo(RectTransform rt, float normalizedRadius)
        {
            const int segments = 100;
            var points = new Vector3[segments + 1];

            float rx = rt.rect.width * 0.5f * normalizedRadius;
            float ry = rt.rect.height * 0.5f * normalizedRadius;
            Vector3 center = rt.position - Vector3.forward;

            for (int i = 0; i <= segments; i++)
            {
                float ang = 2f * Mathf.PI * i / segments;
                float x = Mathf.Cos(ang) * rx;
                float y = Mathf.Sin(ang) * ry;
                Vector3 local = new Vector3(x, y, 0f);
                points[i] = center + rt.rotation * local;
            }

            for (int i = 0; i < segments; i++)
                Gizmos.DrawLine(points[i], points[i + 1]);
        }

        #endregion
#endif
    }
}
