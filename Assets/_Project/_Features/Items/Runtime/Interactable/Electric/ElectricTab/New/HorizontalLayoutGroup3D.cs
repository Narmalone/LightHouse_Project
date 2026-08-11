using UnityEngine;

namespace LightHouse.Features.Tutorial
{
    /// <summary>
    /// Dispose automatiquement les enfants de ce Transform le long d'un axe,
    /// à la manière d'un HorizontalLayoutGroup mais pour des objets 3D.
    /// Fonctionne en éditeur (ExecuteAlways) : tout changement de spacing,
    /// padding, alignement ou hiérarchie ré-arrange immédiatement les enfants.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class HorizontalLayoutGroup3D : MonoBehaviour
    {
        public enum Axis { X, Y, Z }
        public enum Alignment { Start, Center, End }

        [Header("Axe de disposition")]
        [SerializeField] private Axis axis = Axis.X;
        [SerializeField] private Alignment alignment = Alignment.Center;

        [Header("Espacement")]
        [Tooltip("Distance entre deux enfants (ou entre leurs bords si 'Use Child Bounds' est actif).")]
        [SerializeField] private float spacing = 1f;
        [Tooltip("Décalage appliqué avant le premier élément, sur l'axe choisi.")]
        [SerializeField] private float padding = 0f;

        [Header("Options")]
        [Tooltip("Si activé, utilise la taille réelle (bounds du Renderer) de chaque enfant pour l'espacement, au lieu d'un espacement fixe entre pivots.")]
        [SerializeField] private bool useChildBounds = false;
        [SerializeField] private bool reverseOrder = false;
        [Tooltip("Ré-arrange automatiquement dès qu'une valeur change ou qu'un enfant est ajouté/retiré.")]
        [SerializeField] private bool autoArrange = true;

        private void OnValidate()
        {
            if (autoArrange) Arrange();
        }

        private void OnTransformChildrenChanged()
        {
            if (autoArrange) Arrange();
        }

        [ContextMenu("Arrange Now")]
        public void Arrange()
        {
            int count = transform.childCount;
            if (count == 0) return;

            float[] sizes = new float[count];
            float totalSize = 0f;

            for (int i = 0; i < count; i++)
            {
                float size = useChildBounds ? GetChildSize(transform.GetChild(i)) : 0f;
                sizes[i] = size;
                totalSize += size;
                if (i < count - 1) totalSize += spacing;
            }

            float startOffset;
            switch (alignment)
            {
                case Alignment.Start:
                    startOffset = padding;
                    break;
                case Alignment.End:
                    startOffset = -totalSize - padding;
                    break;
                default: // Center
                    startOffset = -totalSize / 2f;
                    break;
            }

            float current = startOffset;
            for (int idx = 0; idx < count; idx++)
            {
                int i = reverseOrder ? count - 1 - idx : idx;
                Transform child = transform.GetChild(i);
                float halfSize = useChildBounds ? sizes[i] / 2f : 0f;
                float pos = current + halfSize;

                Vector3 localPos = child.localPosition;
                SetAxisComponent(ref localPos, pos);
                child.localPosition = localPos;

                current += (useChildBounds ? sizes[i] : 0f) + spacing;
            }
        }

        private float GetChildSize(Transform child)
        {
            Renderer rend = child.GetComponentInChildren<Renderer>();
            if (rend == null) return 0f;

            Bounds b = rend.bounds;
            switch (axis)
            {
                case Axis.X: return b.size.x;
                case Axis.Y: return b.size.y;
                default: return b.size.z;
            }
        }

        private void SetAxisComponent(ref Vector3 v, float value)
        {
            switch (axis)
            {
                case Axis.X: v.x = value; break;
                case Axis.Y: v.y = value; break;
                default: v.z = value; break;
            }
        }
    }
}