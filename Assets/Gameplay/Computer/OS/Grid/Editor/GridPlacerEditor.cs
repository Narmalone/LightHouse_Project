/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridPlacer))]
public class GridPlacerEditor : Editor
{
    private void OnSceneGUI()
    {
        GridPlacer placer = (GridPlacer)target;
        RectTransform rect = placer.GetComponent<RectTransform>();

        if (!rect) return;

        Vector2 size = placer.cellSize;
        int rows = placer.rows;
        int cols = placer.columns;

        Vector3 origin = Vector3.zero;

        switch (placer.origin)
        {
            case GridOrigin.TopLeft:
                origin = Vector3.zero;
                break;
            case GridOrigin.TopRight:
                origin = new Vector3(-cols * size.x, 0, 0);
                break;
            case GridOrigin.BottomLeft:
                origin = new Vector3(0, rows * size.y, 0);
                break;
            case GridOrigin.BottomRight:
                origin = new Vector3(-cols * size.x, rows * size.y, 0);
                break;
        }

        Handles.matrix = rect.localToWorldMatrix;
        Handles.color = Color.cyan;

        for (int r = 0; r <= rows; r++)
        {
            Vector3 start = origin + new Vector3(0, -r * size.y, 0);
            Vector3 end = start + new Vector3(cols * size.x, 0, 0);
            Handles.DrawLine(start, end);
        }

        for (int c = 0; c <= cols; c++)
        {
            Vector3 start = origin + new Vector3(c * size.x, 0, 0);
            Vector3 end = start + new Vector3(0, -rows * size.y, 0);
            Handles.DrawLine(start, end);
        }

        Handles.matrix = Matrix4x4.identity;
    }

}
*/