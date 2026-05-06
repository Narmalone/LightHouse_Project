using UnityEngine;
using UnityEngine.UI;

public enum GameMode
{
    Preset,
    Random
}

public class GameController : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private GridView gridView;
    [SerializeField] private ChronometerController chronometer;

    [Header("Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button randomButton;
    [SerializeField] private Button restartButton;

    [Header("Configs")]
    [SerializeField] private MineSweeperConfig[] presetsConfig;
    [SerializeField] private MineSweeperConfig[] randomConfigs;

    private GameMode gameMode;
    private int selectedPresetIndex = 0;
    private MineSweeperConfig selectedConfig;

    private GameLogic logic;
    private GridData grid;

    private void Awake()
    {
        // Bind UI
        easyButton.onClick.AddListener(() => StartGame(GameMode.Preset, 0));
        mediumButton.onClick.AddListener(() => StartGame(GameMode.Preset, 1));
        hardButton.onClick.AddListener(() => StartGame(GameMode.Preset, 2));
        randomButton.onClick.AddListener(() => StartGame(GameMode.Random));
        restartButton.onClick.AddListener(RestartGame);
    }

    private void Start()
    {
        StartGame(GameMode.Preset, 0); // Start par défaut en Easy
    }

    private void OnDestroy()
    {
        easyButton.onClick.RemoveAllListeners();
        mediumButton.onClick.RemoveAllListeners();
        hardButton.onClick.RemoveAllListeners();
        randomButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
    }

    // =========================
    // GAME FLOW
    // =========================

    public void StartGame(GameMode mode, int presetIndex = 0)
    {
        Cleanup();

        gameMode = mode;
        selectedPresetIndex = presetIndex;

        Initialize();
    }

    private void RestartGame()
    {
        StartGame(gameMode, selectedPresetIndex);
    }

    private void Initialize()
    {
        selectedConfig = GetConfigBasedOnMode();

        int rows = selectedConfig.NumberOfRows;
        int columns = selectedConfig.NumberOfColumns;

        if (selectedConfig.EnableRandomRows)
        {
            rows = Random.Range(selectedConfig.MinNumberOfRows, selectedConfig.MaxNumberOfRows + 1);
        }

        if (selectedConfig.EnableRandomColumns)
        {
            columns = Random.Range(selectedConfig.MinNumberOfColumns, selectedConfig.MaxNumberOfColumns + 1);
        }

        grid = new GridData(columns, rows);
        logic = new GameLogic();

        logic.Initialize(grid);
        gridView.Initialize(grid);
        chronometer.Initialize();

        SubscribeEvents();
        ConnectInput();
    }

    private void Cleanup()
    {
        if (logic != null)
        {
            logic.OnCellRevealed -= HandleReveal;
            logic.OnCellFlagged -= HandleFlag;
            logic.OnGameLost -= Logic_OnGameLost;
            logic.OnGameWon -= Logic_OnGameWon;
        }

        if (gridView != null)
        {
            gridView.Clear();
        }
    }

    // =========================
    // CONFIG
    // =========================

    private MineSweeperConfig GetConfigBasedOnMode()
    {
        switch (gameMode)
        {
            case GameMode.Preset:
                return presetsConfig[selectedPresetIndex];

            case GameMode.Random:
                return randomConfigs[Random.Range(0, randomConfigs.Length)];

            default:
                return presetsConfig[0];
        }
    }

    // =========================
    // EVENTS
    // =========================

    private void SubscribeEvents()
    {
        logic.OnCellRevealed += HandleReveal;
        logic.OnCellFlagged += HandleFlag;
        logic.OnGameLost += Logic_OnGameLost;
        logic.OnGameWon += Logic_OnGameWon;
    }

    private void ConnectInput()
    {
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

    private void Logic_OnGameWon()
    {
        Debug.Log("WIN");
        chronometer.Stop();
    }

    private void Logic_OnGameLost()
    {
        Debug.Log("LOSE");
        chronometer.Stop();
    }
}