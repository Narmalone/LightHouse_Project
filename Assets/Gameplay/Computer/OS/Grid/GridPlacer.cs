using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class DesktopGrid : MonoBehaviour
{
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = new Vector2(10, 10);
    public int columns = 6;
    public int rows = 6;
    public bool autoUpdate = true;

    private RectTransform rectTransform;

    private void Update()
    {
        if (!Application.isPlaying && autoUpdate)
        {
            UpdateGrid();
        }
    }

    public void UpdateGrid()
    {
        rectTransform = GetComponent<RectTransform>();
        Vector2 rectSize = rectTransform.rect.size;

        // Calcul de l’origine haut gauche dans le repère local
        Vector2 topLeftOrigin = new Vector2(
            -rectSize.x * rectTransform.pivot.x,
            rectSize.y * (1 - rectTransform.pivot.y)
        );

        foreach (Transform child in transform)
        {
            var icon = child.GetComponent<DesktopIcon>();
            if (icon == null) continue;

            int index = icon.gridIndex;
            int row = index / columns;
            int col = index % columns;

            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect == null) continue;

            // Ajout du spacing
            Vector2 pos = new Vector2(
                col * (cellSize.x + spacing.x),
                -row * (cellSize.y + spacing.y)
            );

            childRect.anchorMin = childRect.anchorMax = new Vector2(0, 0);
            childRect.pivot = new Vector2(0, 1); // top-left alignement
            childRect.sizeDelta = cellSize;
            childRect.anchoredPosition = topLeftOrigin + pos;
        }
    }
}
