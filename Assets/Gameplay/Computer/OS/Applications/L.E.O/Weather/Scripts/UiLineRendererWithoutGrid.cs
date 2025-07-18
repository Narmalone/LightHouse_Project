using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRendererWithoutGrid : Graphic
{
    public List<Vector2> points;
    public float thickness = 10f;
    public bool center = true;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Count < 2)
            return;

        Vector2 offset = center ? GetCenterOffset() : Vector2.zero;

        for (int i = 0; i < points.Count - 1; i++)
        {
            int baseIndex = vh.currentVertCount;
            CreateLineSegment(points[i], points[i + 1], offset, vh);

            // Body triangles
            vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 3);
            vh.AddTriangle(baseIndex + 3, baseIndex + 2, baseIndex + 0);

            // Bevel joints (optional)
            if (i > 0)
            {
                vh.AddTriangle(baseIndex + 0, baseIndex - 1, baseIndex - 3);
                vh.AddTriangle(baseIndex + 1, baseIndex - 1, baseIndex - 2);
            }
        }
    }

    private Vector2 GetCenterOffset()
    {
        Rect pixelRect = GetPixelAdjustedRect();
        return new Vector2(pixelRect.width / 2f, pixelRect.height / 2f);
    }

    private void CreateLineSegment(Vector2 start, Vector2 end, Vector2 offset, VertexHelper vh)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness / 2f);

        // Define vertices
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        vert.position = start - normal - offset;
        vh.AddVert(vert);
        vert.position = start + normal - offset;
        vh.AddVert(vert);
        vert.position = end - normal - offset;
        vh.AddVert(vert);
        vert.position = end + normal - offset;
        vh.AddVert(vert);

        // Center vertex (optional)
        vert.position = end - offset;
        vh.AddVert(vert);
    }
}
