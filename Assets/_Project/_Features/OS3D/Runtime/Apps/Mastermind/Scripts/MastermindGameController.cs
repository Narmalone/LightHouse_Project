using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.Mastermind
{
    public enum MastermindColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange,
        Pink,
        Cyan
    }

    public class MastermindGameController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private MastermindSettings _selectedSettings;
        [SerializeField] private MastermindBoardView _boardView;
        [SerializeField] private PlayerGuessColorsBoardView _playerBoardView;
        [SerializeField] private InfoPanelMenuController _infoPanelMenuController;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _randomButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _selectDifficultyMenuButton;

        [SerializeField] private DifficultyMenuController _difficultyMenuController;

        [SerializeField] private GameObject _difficultyMenu;

        [Header("Configs")]
        [SerializeField] private MastermindSettings[] _presetsConfig;


        private MastermindGameLogic _gameLogic;

        /// <summary>
        /// Current editable player guess.
        /// Null = empty slot.
        /// </summary>
        private MastermindColor?[] _currentGuess;

        /// <summary>
        /// Currently selected color
        /// from the palette.
        /// </summary>
        private MastermindColor _selectedColor;

        #endregion

        #region Properties

        public MastermindGameData GameData =>
            _gameLogic.GameData;

        public MastermindColor[] AvailableColors =>
            _selectedSettings.AvailableColors;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _submitButton.onClick
                .AddListener(
                    SubmitCurrentGuess);

            _selectDifficultyMenuButton.onClick.AddListener(OnSelectDifficultyClicked);
            _randomButton.onClick.AddListener(OnRandomClicked);
            _restartButton.onClick.AddListener(OnRestartClicked);
            _difficultyMenuController.OnDifficultySelected += DifficultyMenuController_OnDifficultySelected;
        }

        private void DifficultyMenuController_OnDifficultySelected(int selectedDifficulty)
        {
            StartGame(selectedDifficulty);
            _infoPanelMenuController.Hide();
        }

        private void OnRestartClicked()
        {
            // Restart current preset.
            StartGame(
                System.Array.IndexOf(
                    _presetsConfig,
                    _selectedSettings));
        }

        private void OnRandomClicked()
        {
            int randomIndex = Random.Range(0, _presetsConfig.Length);
            StartGame(randomIndex);
        }

        private void OnSelectDifficultyClicked()
        {
            _difficultyMenu.SetActive(true);
        }

        private void OnDestroy()
        {
            _submitButton.onClick
                .RemoveListener(
                    SubmitCurrentGuess);

            _selectDifficultyMenuButton.onClick.RemoveListener(OnSelectDifficultyClicked);
            _randomButton.onClick.RemoveListener(OnRandomClicked);
            _restartButton.onClick.RemoveListener(OnRestartClicked);
            _difficultyMenuController.OnDifficultySelected -= DifficultyMenuController_OnDifficultySelected;
        }

        #endregion

        #region Initialization

        public void Initialize()
        {
            StartGame(0);
        }

        /// <summary>
        /// Starts a new game using a preset index.
        /// 0 = Easy
        /// 1 = Medium
        /// 2 = Hard
        /// etc...
        /// </summary>
        private void StartGame(int selectedIndex)
        {
            // Prevent invalid indexes.
            if (selectedIndex < 0 ||
                selectedIndex >= _presetsConfig.Length)
            {
                Debug.LogWarning(
                    $"Invalid preset index : {selectedIndex}");

                return;
            }

            // Unbind previous runtime events
            // before recreating a game.
            UnsubscribeEvents();

            // Select runtime config.
            _selectedSettings =
                _presetsConfig[selectedIndex];

            // Create fresh logic instance.
            _gameLogic =
                new MastermindGameLogic();

            _gameLogic.Initialize(
                _selectedSettings);

            // Regenerate board based on:
            // - code size
            // - max attempts
            _boardView.GenerateRows(
                _selectedSettings.MaxAttempts,
                _selectedSettings.CodeLength);

            // Regenerate bottom palette.
            _playerBoardView.Initialize(
                _selectedSettings);

            // Create new empty runtime guess.
            CreateEmptyGuess();

            // Rebind events to new rows.
            BindEvents();

            // Refresh editable row state.
            RefreshRowsInteractivity();

            // Hide difficulty popup.
            _difficultyMenu.SetActive(false);
        }

        private void BindEvents()
        {
            _playerBoardView.OnColorSelected +=
                SelectColor;

            _gameLogic.OnGuessValidated +=
                HandleGuessValidated;

            SubscribeToCurrentRow();
        }

        private void UnsubscribeEvents()
        {
            _playerBoardView.OnColorSelected -=
                SelectColor;

            if (_gameLogic != null)
            {
                _gameLogic.OnGuessValidated -=
                    HandleGuessValidated;
            }

            UnsubscribeFromCurrentRow();
        }

        #endregion

        #region Rows Subscription

        private void SubscribeToCurrentRow()
        {
            // No game running yet.
            if (_gameLogic == null)
                return;

            if (_gameLogic.GameData == null)
                return;

            MastermindRowView row =
                _boardView.GetRow(
                    _gameLogic.GameData.CurrentTurn);

            if (row == null)
                return;

            row.OnGuessSlotClicked +=
                HandleGuessSlotClicked;
        }

        private void UnsubscribeFromCurrentRow()
        {
            // No game running yet.
            if (_gameLogic == null)
                return;

            if (_gameLogic.GameData == null)
                return;

            MastermindRowView row =
                _boardView.GetRow(
                    _gameLogic.GameData.CurrentTurn);

            if (row == null)
                return;

            row.OnGuessSlotClicked -=
                HandleGuessSlotClicked;
        }

        #endregion

        #region Guess Management

        private void CreateEmptyGuess()
        {
            _currentGuess =
                new MastermindColor?[
                    _selectedSettings.CodeLength];
        }

        public void SelectColor(
            MastermindColor color)
        {
            _selectedColor = color;
        }

        private void HandleGuessSlotClicked(
            int index)
        {
            SetColorAtIndex(index);
        }

        public void SetColorAtIndex(int index)
        {
            if (index < 0 ||
                index >= _currentGuess.Length)
                return;

            _currentGuess[index] =
                _selectedColor;

            MastermindRowView row =
                _boardView.GetRow(
                    _gameLogic.GameData.CurrentTurn);

            if (row == null)
                return;

            row.SetGuessSlotColor(
                index,
                _selectedSettings.GetVisualColor(
                    _selectedColor));
        }

        public void SubmitCurrentGuess()
        {
            if (_currentGuess == null)
                return;

            if (_currentGuess.Length !=
                _selectedSettings.CodeLength)
                return;

            UnsubscribeFromCurrentRow();

            _gameLogic.SubmitGuess(
                _currentGuess);

            CreateEmptyGuess();

            RefreshRowsInteractivity();

            SubscribeToCurrentRow();
        }

        #endregion

        #region Guess Result

        private void HandleGuessValidated(
            int rowIndex,
            MastermindHint hint)
        {
            MastermindRowView row =
                _boardView.GetRow(rowIndex);

            if (row == null)
                return;

            row.SetHints(hint.Hints);
        }

        #endregion

        #region Rows State

        private void RefreshRowsInteractivity()
        {
            for (int i = 0;
                 i < _boardView.Rows.Count;
                 i++)
            {
                bool isCurrentRow =
                    i ==
                    _gameLogic.GameData.CurrentTurn;

                _boardView.Rows[i]
                    .SetInteractable(
                        isCurrentRow);
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Random Guess")]
        private void PickRandomColor()
        {
            for (int i = 0;
                 i < _selectedSettings.CodeLength;
                 i++)
            {
                int randomIndex =
                    Random.Range(
                        0,
                        _selectedSettings
                            .AvailableColors.Length);

                _currentGuess[i] =
                    _selectedSettings
                        .AvailableColors[randomIndex];
            }
        }

        private void Reveal()
        {
            foreach(var color in _gameLogic.GameData.SecretCode)
            {
                Debug.Log(color.ToString());

            }
        }

        #endregion

        private void ShowExplanationPanel()
        {
                _infoPanelMenuController.Initialize(
                    new string[]
                    {
                        "Welcome to Mastermind!",
                        "The goal of the game is to guess the secret code.",
                        "The code is made of a sequence of colors.",
                        "Each turn, you can submit a guess.",
                        "After each guess, you'll receive hints to help you find the secret code.",
                        "Good luck!",

                        "Max Attempts: " + _selectedSettings.MaxAttempts,
                        "Code Length: " + _selectedSettings.CodeLength,
                        "Number of colors: " + _selectedSettings.AvailableColors.Length,
                        "Duplicate colors: " + (_selectedSettings.AllowDuplicateColors ? "Yes" : "No"),
                    });
        }
    }
}