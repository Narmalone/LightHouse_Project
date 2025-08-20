using UnityEngine.Pool;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Inline", 82)]
    public class Inline : Shadow
    {
        protected Inline() { }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            int count = verts.Count;
            var neededCapacity = count * 5;
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            // Negative offsets to push toward the "inside"
            var offset = -effectDistance;

            int start = 0;
            int end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, offset.x, offset.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, offset.x, -offset.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -offset.x, offset.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -offset.x, -offset.y);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}
