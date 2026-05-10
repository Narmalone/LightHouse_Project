using System;
using System.Collections.Generic;

namespace LightHouse.Features.Computer.Mastermind
{
    public enum MastermindHintType
    {
        Empty,
        Exact,
        Partial
    }

    public struct MastermindHint
    {
        /// <summary>
        /// Final visual hints shown to the player.
        /// </summary>
        public List<MastermindHintType> Hints;
    }

    public class MastermindGameLogic
    {
        #region Events

        public event Action<int, MastermindHint>
            OnGuessValidated;

        public event Action OnGameWon;

        public event Action OnGameLost;

        #endregion

        #region Fields

        private bool _isInitialized;

        private MastermindSettings _settings;

        private MastermindGameData _gameData;

        #endregion

        #region Properties

        public bool IsInitialized => _isInitialized;

        public MastermindGameData GameData =>
            _gameData;

        #endregion

        #region Initialization

        public void Initialize(
            MastermindSettings settings)
        {
            _settings = settings;

            _gameData =
                new MastermindGameData();

            _gameData.CurrentTurn = 0;

            _gameData.IsGameOver = false;

            GenerateSecretCode();

            _isInitialized = true;
        }

        #endregion

        #region Public API

        public void SubmitGuess(
            MastermindColor?[] guess)
        {
            if (!_isInitialized)
                return;

            if (_gameData.IsGameOver)
                return;

            if (guess == null)
                return;

            if (guess.Length !=
                _settings.CodeLength)
                return;

            MastermindHint hint =
                CompareGuess(
                    guess,
                    _gameData.SecretCode);

            _gameData.GuessesHistory.Add(
                 (MastermindColor?[])guess.Clone());

            _gameData.HintsHistory.Add(hint);

            OnGuessValidated?.Invoke(
                _gameData.CurrentTurn,
                hint);

            bool isVictory = true;

            // Victory only if ALL hints
            // are exact matches.
            for (int i = 0;
                 i < hint.Hints.Count;
                 i++)
            {
                if (hint.Hints[i] !=
                    MastermindHintType.Exact)
                {
                    isVictory = false;
                    break;
                }
            }

            if (isVictory)
            {
                _gameData.IsGameOver = true;

                OnGameWon?.Invoke();

                return;
            }

            // Increment ONLY ONCE per guess.
            _gameData.CurrentTurn++;

            bool hasLost =
                _gameData.CurrentTurn >=
                _settings.MaxAttempts;

            if (hasLost)
            {
                _gameData.IsGameOver = true;

                OnGameLost?.Invoke();
            }
        }

        #endregion

        #region Secret Code

        private void GenerateSecretCode()
        {
            _gameData.SecretCode =
                new MastermindColor[
                    _settings.CodeLength];

            List<int> usedIndexes = new();

            for (int i = 0;
                 i < _settings.CodeLength;
                 i++)
            {
                int randomIndex;

                do
                {
                    randomIndex =
                        UnityEngine.Random.Range(
                            0,
                            _settings.AvailableColors.Length);
                }
                while (
                    !_settings.AllowDuplicateColors &&
                    usedIndexes.Contains(randomIndex));

                usedIndexes.Add(randomIndex);

                _gameData.SecretCode[i] =
                    _settings.AvailableColors[randomIndex];
            }
        }

        #endregion

        #region Comparison

        private MastermindHint CompareGuess(
            MastermindColor?[] guess,
            MastermindColor[] secret)
        {
            List<MastermindHintType> hints =
                new();

            bool[] secretMatched =
                new bool[secret.Length];

            bool[] guessMatched =
                new bool[guess.Length];

            // PASS 1
            // Exact matches
            for (int i = 0;
                 i < guess.Length;
                 i++)
            {
                // Empty slots are ignored.
                if (!guess[i].HasValue)
                    continue;

                if (guess[i] == secret[i])
                {
                    hints.Add(
                        MastermindHintType.Exact);

                    secretMatched[i] = true;

                    guessMatched[i] = true;
                }
            }

            // PASS 2
            // Partial matches
            for (int guessIndex = 0;
                 guessIndex < guess.Length;
                 guessIndex++)
            {
                if (guessMatched[guessIndex])
                    continue;

                // Empty slots are ignored.
                if (!guess[guessIndex].HasValue)
                    continue;

                for (int secretIndex = 0;
                     secretIndex < secret.Length;
                     secretIndex++)
                {
                    if (secretMatched[secretIndex])
                        continue;

                    if (guess[guessIndex] ==
                        secret[secretIndex])
                    {
                        hints.Add(
                            MastermindHintType.Partial);

                        secretMatched[secretIndex] =
                            true;

                        guessMatched[guessIndex] =
                            true;

                        break;
                    }
                }
            }

            // Fill remaining slots
            // with empty hints.
            while (hints.Count <
                   _settings.CodeLength)
            {
                hints.Add(
                    MastermindHintType.Empty);
            }

            // Shuffle hints so the player
            // doesn't know WHICH slot
            // was correct.
            for (int i = 0;
                 i < hints.Count;
                 i++)
            {
                int randomIndex =
                    UnityEngine.Random.Range(
                        i,
                        hints.Count);

                (hints[i], hints[randomIndex]) =
                    (hints[randomIndex], hints[i]);
            }

            return new MastermindHint
            {
                Hints = hints
            };
        }

        #endregion
    }
}