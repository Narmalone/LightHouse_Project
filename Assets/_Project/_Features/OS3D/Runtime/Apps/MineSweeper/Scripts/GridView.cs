using UnityEngine;
using UnityEngine.UI;

public class GridView : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private Transform parent;

    [SerializeField] private Sprite hidden;
    [SerializeField] private Sprite mine;
    [SerializeField] private Sprite flag;
    [SerializeField] private Color[] numberColors;

    private CellView[,] views;
    private GridData grid;

    public void Initialize(GridData data)
    {
        grid = data;

        SetGridLayout(data.Width, data.Height);

        views = new CellView[data.Width, data.Height];

/*        for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
            {
                var view = Instantiate(cellPrefab, parent);
                view.X = x;
                view.Y = y;

                view.SetSprite(hidden);

                views[x, y] = view;
            }*/

        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                var view = Instantiate(cellPrefab, parent);
                view.X = x;
                view.Y = y;

                view.SetSprite(hidden);

                views[x, y] = view;
            }
        }
    }

    public void SetGridLayout(int columns, int rows)
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;

        var rect = gridLayout.GetComponent<RectTransform>();

        float availableWidth = rect.rect.width;
        float availableHeight = rect.rect.height;

        float cellWidth = availableWidth / columns;
        float cellHeight = availableHeight / rows;

        float size = Mathf.Min(cellWidth, cellHeight);

        gridLayout.cellSize = new Vector2(size, size);
    }

    public void Clear()
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    public CellView GetCellView(int x, int y)
        => views[x, y];

    public void UpdateCell(int x, int y)
    {
        var cell = grid.GetCell(x, y);
        var view = views[x, y];

        if (cell.IsFlagged && !cell.IsRevealed)
        {
            view.SetSprite(flag);
            view.SetBackgroundColor(Color.white);
            view.SetText("");
            return;
        }

        if (!cell.IsRevealed)
        {
            // case NON révélée = bouton
            view.SetSprite(hidden);
            view.SetBackgroundColor(Color.white); // ou ton sprite fait déjà le style
            view.SetIcondColor(Color.white); // ou ton sprite fait déjà le style
            view.SetText("");
            return;
        }

        // case révélée = fond plat gris
        view.SetSprite(null);
        view.SetBackgroundColor(new Color(0.75f, 0.75f, 0.75f)); // gris clair
        view.SetIcondColor(new Color(0.75f, 0.75f, 0.75f)); // ou ton sprite fait déjà le style

        if (cell.IsMine)
        {
            view.SetSprite(mine);
            view.SetText("");
        }
        else
        {
            if (cell.AdjacentMines > 0)
            {
                view.SetText(cell.AdjacentMines.ToString());
                view.SetTextColor(numberColors[cell.AdjacentMines]);
            }
            else
            {
                view.SetText("");
            }
        }
    }
}