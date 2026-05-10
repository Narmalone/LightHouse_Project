using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.MineSweeper
{
    public enum GameMode
    {
        Preset,
        Random
    }

    public class MineSweeperGameController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private GridView gridView;
        [SerializeField] private ChronometerController chronometer;
        [SerializeField] private RestartController restartController;
        [SerializeField] private MineCountController mineCountController;

        [Header("Buttons")]
        [SerializeField] private DifficultyMenuController difficultyMenu;
        [SerializeField] private Button difficultyMenuButton;
        [SerializeField] private Button randomButton;
        [SerializeField] private Button restartButton;

        [Header("Configs")]
        [SerializeField] private MineSweeperConfig[] presetsConfig;
        [SerializeField] private MineSweeperConfig[] randomConfigs;

        private GameMode gameMode;
        private int selectedPresetIndex = 0;

        // 🔥 Runtime state (IMPORTANT)
        private MineSweeperConfig runtimeConfig;
        private int runtimeRows;
        private int runtimeColumns;
        private int runtimeMines;

        private MineSweeperGameLogic logic;
        private GridData grid;

        private void Awake()
        {
            difficultyMenu.OnDifficultySelected += DifficultyMenu_OnDifficultySelected;
            difficultyMenuButton.onClick.AddListener(OnDifficultyMenuButtonClicked);
            randomButton.onClick.AddListener(OnRandomClicked);
            restartButton.onClick.AddListener(RestartGame);
        }

        private void OnRandomClicked()
        {
            StartGame(GameMode.Random);
        }

        private void OnDifficultyMenuButtonClicked()
        {
            difficultyMenu.gameObject.SetActive(true);
        }

        private void Start()
        {
            StartGame(GameMode.Preset, 0);
            restartController.SetAlive();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                RevealAll();
            }
        }

        private void OnDestroy()
        {
            difficultyMenu.OnDifficultySelected -= DifficultyMenu_OnDifficultySelected;
            difficultyMenuButton.onClick.RemoveListener(OnDifficultyMenuButtonClicked);
            randomButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
        }

        private void DifficultyMenu_OnDifficultySelected(int index)
        {
            StartGame(GameMode.Preset, index);
        }

        // =========================
        // GAME FLOW
        // =========================

        public void StartGame(GameMode mode, int presetIndex = 0)
        {
            Cleanup();

            gameMode = mode;
            selectedPresetIndex = presetIndex;

            // 🔥 CRÉATION DU RUNTIME CONFIG UNE SEULE FOIS
            ResolveRuntimeConfig();

            Initialize();
        }

        private void RestartGame()
        {
            Cleanup();
            Initialize(); // 🔥 PAS de re-random ici
        }

        public void Initialize()
        {
            gridView.Clear();
            grid = new GridData(runtimeColumns, runtimeRows);
            logic = new MineSweeperGameLogic();

            restartController.SetAlive();

            logic.Initialize(grid, runtimeMines);
            mineCountController.Initialize(runtimeMines);
            chronometer.StopChrono();

            gridView.Initialize(grid);

            SubscribeEvents();
            ConnectInput();
        }

        private void Cleanup()
        {
            if (logic != null)
            {
                logic.OnCellRevealed -= HandleReveal;
                logic.OnFirstCellRevealed -= Logic_OnFirstCellRevealed;
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
        // RUNTIME CONFIG (🔥 IMPORTANT)
        // =========================

        private void ResolveRuntimeConfig()
        {
            // 1. Choisir la config de base
            if (gameMode == GameMode.Preset)
            {
                runtimeConfig = presetsConfig[selectedPresetIndex];
            }
            else
            {
                runtimeConfig = randomConfigs[Random.Range(0, randomConfigs.Length)];
            }

            runtimeMines = runtimeConfig.EnableRandomMines
            ? Random.Range(runtimeConfig.MinMinesNumber, runtimeConfig.MaxMinesNumber + 1)
            : runtimeConfig.Mines;

            // 2. Résoudre les valeurs (UNE SEULE FOIS)
            runtimeRows = runtimeConfig.EnableRandomRows
                ? Random.Range(runtimeConfig.MinNumberOfRows, runtimeConfig.MaxNumberOfRows + 1)
                : runtimeConfig.NumberOfRows;

            runtimeColumns = runtimeConfig.EnableRandomColumns
                ? Random.Range(runtimeConfig.MinNumberOfColumns, runtimeConfig.MaxNumberOfColumns + 1)
                : runtimeConfig.NumberOfColumns;
        }

        // =========================
        // EVENTS
        // =========================

        private void SubscribeEvents()
        {
            logic.OnCellRevealed += HandleReveal;
            logic.OnFirstCellRevealed += Logic_OnFirstCellRevealed;
            logic.OnCellFlagged += HandleFlag;
            logic.OnGameLost += Logic_OnGameLost;
            logic.OnGameWon += Logic_OnGameWon;
        }

        private void Logic_OnFirstCellRevealed(int arg1, int arg2)
        {
            Debug.Log("first cell revealed, start chrono");
            chronometer.StartChrono();
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

        private void RevealAll()
        {
            if (!logic.IsInitialized)
                logic.Reveal(Random.Range(0, runtimeColumns), Random.Range(0, runtimeRows));

            int mineCount = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.GetCell(x, y);

                    cell.IsRevealed = true;

                    if (cell.IsMine)
                        mineCount++;

                    gridView.UpdateCell(x, y);
                }
            }
            Debug.Log($"DEBUG Mines counted: {mineCount}");
        }

        private void HandleFlag(int x, int y, bool isFlagged)
        {
            gridView.UpdateCell(x, y);
            if (isFlagged)
                mineCountController.OnFlagPut();
            else
                mineCountController.OnFlagRemoved();

        }

        private void Logic_OnGameWon()
        {
            Debug.Log("WIN");
            chronometer.StopChrono();
        }

        private void Logic_OnGameLost()
        {
            Debug.Log("LOSE");
            restartController.SetDead();
            chronometer.StopChrono();
            RevealAll();
        }
    }
}
