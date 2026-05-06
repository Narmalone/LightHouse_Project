using System;

public class GameLogic
{
    private bool isInitialized = false;
    private GridData grid;

    public event Action<int, int> OnCellRevealed;
    public event Action<int, int> OnCellFlagged;
    public event Action OnGameLost;
    public event Action OnGameWon;

    public void Initialize(GridData gridData)
    {
        grid = gridData;
        // TODO : placer mines + calcul adjacence
    }

    public void Reveal(int x, int y)
    {
        if (!grid.IsInside(x, y)) return;

        if (!isInitialized)
        {
            GenerateMines(x, y);
            CalculateAdjacents();
            isInitialized = true;
        }

        var cell = grid.GetCell(x, y);

        if (cell.IsRevealed || cell.IsFlagged)
            return;

        cell.IsRevealed = true;
        OnCellRevealed?.Invoke(x, y);

        if (cell.IsMine)
        {
            OnGameLost?.Invoke();
            return;
        }

        if (cell.AdjacentMines == 0)
        {
            FloodFill(x, y);
        }

        if (CheckWin())
        {
            OnGameWon?.Invoke();
        }
    }

    private void GenerateMines(int safeX, int safeY)
    {
        var random = new System.Random();
        var positions = new System.Collections.Generic.List<(int, int)>();

        // construire toutes les positions valides
        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
            {
                if (x == safeX && y == safeY)
                    continue;

                positions.Add((x, y));
            }

        // mélanger (Fisher-Yates)
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (positions[i], positions[j]) = (positions[j], positions[i]);
        }

        int minesToPlace = 10;

        for (int i = 0; i < minesToPlace; i++)
        {
            var (x, y) = positions[i];
            grid.GetCell(x, y).IsMine = true;
        }
    }

    private void CalculateAdjacents()
    {
        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
            {
                var cell = grid.GetCell(x, y);

                if (cell.IsMine) continue;

                int count = 0;

                foreach (var (nx, ny) in GetNeighbors(x, y))
                {
                    if (grid.GetCell(nx, ny).IsMine)
                        count++;
                }

                cell.AdjacentMines = count;
            }
    }

    public void ToggleFlag(int x, int y)
    {
        if (!grid.IsInside(x, y)) return;

        var cell = grid.GetCell(x, y);

        if (cell.IsRevealed) return;

        cell.IsFlagged = !cell.IsFlagged;
        OnCellFlagged?.Invoke(x, y);
    }

    private void FloodFill(int startX, int startY)
    {
        var queue = new System.Collections.Generic.Queue<(int, int)>();
        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            foreach (var (nx, ny) in GetNeighbors(x, y))
            {
                var neighbor = grid.GetCell(nx, ny);

                if (neighbor.IsRevealed || neighbor.IsMine)
                    continue;

                neighbor.IsRevealed = true;
                OnCellRevealed?.Invoke(nx, ny);

                if (neighbor.AdjacentMines == 0)
                    queue.Enqueue((nx, ny));
            }
        }
    }

    private System.Collections.Generic.IEnumerable<(int, int)> GetNeighbors(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (grid.IsInside(nx, ny))
                    yield return (nx, ny);
            }
    }

    private bool CheckWin()
    {
        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
            {
                var cell = grid.GetCell(x, y);
                if (!cell.IsMine && !cell.IsRevealed)
                    return false;
            }
        return true;
    }
}