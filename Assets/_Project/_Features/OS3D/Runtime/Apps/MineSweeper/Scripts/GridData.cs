public class GridData
{
    private readonly CellData[,] grid;
    public CellData[,] CELLS => grid;

    public int Width { get; }
    public int Height { get; }

    public GridData(int width, int height)
    {
        Width = width;
        Height = height;
        grid = new CellData[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new CellData();
    }

    public CellData GetCell(int x, int y) => grid[x, y];

    public bool IsInside(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height;
}