using UnityEngine;
using System.Collections.Generic;

public class GridView : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private Transform parent;

    [SerializeField] private Sprite hidden;
    [SerializeField] private Sprite mine;
    [SerializeField] private Color[] numberColors;

    private CellView[,] views;
    private GridData grid;

    public void Initialize(GridData data)
    {
        grid = data;
        views = new CellView[data.Width, data.Height];

        for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
            {
                var view = Instantiate(cellPrefab, parent);
                view.X = x;
                view.Y = y;

                view.SetSprite(hidden);

                views[x, y] = view;
            }
    }

    public CellView GetCellView(int x, int y)
        => views[x, y];

    public void UpdateCell(int x, int y)
    {
        var cell = grid.GetCell(x, y);
        var view = views[x, y];

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