using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GridView gridView;

    private GameLogic logic;
    private GridData grid;

    private void Start()
    {
        grid = new GridData(9, 9);
        logic = new GameLogic();

        logic.Initialize(grid);
        gridView.Initialize(grid);

        // SUBSCRIBE
        logic.OnCellRevealed += HandleReveal;
        logic.OnCellFlagged += HandleFlag;
        logic.OnGameLost += () => Debug.Log("LOSE");
        logic.OnGameWon += () => Debug.Log("WIN");

        // CONNECT INPUT
        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
            {
                var view = gridView.GetCellView(x, y);

                view.OnClicked += logic.Reveal;
                view.OnRightClicked += logic.ToggleFlag;
            }
    }

    private void HandleReveal(int x, int y)
    {
        gridView.UpdateCell(x, y);
    }

    private void HandleFlag(int x, int y)
    {
        gridView.UpdateCell(x, y);
    }
}