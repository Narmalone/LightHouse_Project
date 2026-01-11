using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.UI
{
    /// <summary>
    /// Petit renderer de polyline 2D (UI) sans grille :
    /// - Rend chaque segment comme un quad (épaisseur en pixels)
    /// - Option de centrage (points exprimés autour du centre du RectTransform)
    /// - Boucle optionnelle (ferme la polyline)
    /// - API utilitaire : SetPoints / AddPoint / ClearPoints
    /// 
    /// NOTE : Implémentation volontairement simple ? pas de joins "miter/bevel".
    /// Pour la plupart des cas UI (graphes), cela suffit et évite les artefacts d’indices.
    /// </summary>
    [AddComponentMenu("LightHouse/UI/UILineRendererWithoutGrid")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UILineRendererWithoutGrid : Graphic
    {
        #region Serialized Fields

        [Header("Polyline")]
        [Tooltip("Points dans l’espace local du RectTransform (si 'Center' est vrai, ils sont centrés).")]
        [SerializeField] private List<Vector2> _points = new();

        [Tooltip("Épaisseur de la ligne en pixels.")]
        [Min(0.1f)]
        [SerializeField] private float _thickness = 2f;

        [Tooltip("Si vrai, les points seront rendus autour du centre du RectTransform.")]
        [SerializeField] private bool _center = true;

        [Tooltip("Si vrai, relie le dernier point au premier pour fermer la ligne.")]
        [SerializeField] private bool _closedLoop = false;

        #endregion

        #region Public API

        /// <summary>Accès lecture seule aux points.</summary>
        public IReadOnlyList<Vector2> Points => _points;

        public float Thickness
        {
            get => _thickness;
            set { _thickness = Mathf.Max(0.1f, value); SetVerticesDirty(); }
        }

        public bool Center
        {
            get => _center;
            set { _center = value; SetVerticesDirty(); }
        }

        public bool ClosedLoop
        {
            get => _closedLoop;
            set { _closedLoop = value; SetVerticesDirty(); }
        }

        /// <summary>Remplace la liste des points et reconstruit le mesh.</summary>
        public void SetPoints(List<Vector2> points)
        {
            _points = points ?? new List<Vector2>();
            SetVerticesDirty();
        }

        /// <summary>Ajoute un point et reconstruit le mesh.</summary>
        public void AddPoint(Vector2 p)
        {
            _points.Add(p);
            SetVerticesDirty();
        }

        /// <summary>Vide la polyline et reconstruit le mesh.</summary>
        public void ClearPoints()
        {
            _points.Clear();
            SetVerticesDirty();
        }

        #endregion

        #region Graphic

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_points == null) return;

            int count = _points.Count;
            if (count < 2) return;

            Vector2 offset = _center ? GetCenterOffset() : Vector2.zero;

            // Nombre de segments (si loop ? autant que de points, sinon n-1)
            int segmentCount = _closedLoop ? count : count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                int next = (i + 1) % count;
                AddSegmentQuad(vh, _points[i], _points[next], offset, _thickness, color);
            }
        }

        #endregion

        #region Internals

        private Vector2 GetCenterOffset()
        {
            Rect r = GetPixelAdjustedRect();
            return new Vector2(r.width * 0.5f, r.height * 0.5f);
        }

        /// <summary>
        /// Ajoute un quad pour le segment [a,b] avec l’épaisseur donnée.
        /// </summary>
        private static void AddSegmentQuad(
            VertexHelper vh,
            Vector2 a, Vector2 b,
            Vector2 offset,
            float thickness,
            Color32 col)
        {
            Vector2 dir = (b - a);
            if (dir.sqrMagnitude < 1e-6f) return;

            dir.Normalize();
            Vector2 n = new(-dir.y, dir.x); // normale à gauche
            Vector2 half = n * (thickness * 0.5f);

            // Quatre sommets du quad (contre-offset si centré)
            Vector3 v0 = a - half - offset;
            Vector3 v1 = a + half - offset;
            Vector3 v2 = b - half - offset;
            Vector3 v3 = b + half - offset;

            int baseIndex = vh.currentVertCount;

            AddVert(vh, v0, col);
            AddVert(vh, v1, col);
            AddVert(vh, v2, col);
            AddVert(vh, v3, col);

            // Deux triangles : (0,1,3) et (3,2,0)
            vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 3);
            vh.AddTriangle(baseIndex + 3, baseIndex + 2, baseIndex + 0);
        }

        private static void AddVert(VertexHelper vh, Vector3 pos, Color32 col)
        {
            UIVertex v = UIVertex.simpleVert;
            v.color = col;
            v.position = pos;
            vh.AddVert(v);
        }

        #endregion
    }
}
