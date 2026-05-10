using System.Collections.Generic;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Runtime state of the current Mastermind game.
    /// </summary>
    public class MastermindGameData
    {
        /// <summary>
        /// Secret code the player must guess.
        /// </summary>
        public MastermindColor[] SecretCode;

        /// <summary>
        /// All submitted guesses.
        /// </summary>
        public readonly List<MastermindColor?[]> GuessesHistory = new();
        /// <summary>
        /// All generated hints.
        /// </summary>
        public readonly List<MastermindHint> HintsHistory = new();

        /// <summary>
        /// Current turn index.
        /// </summary>
        public int CurrentTurn;

        /// <summary>
        /// Whether the game is over.
        /// </summary>
        public bool IsGameOver;
    }
}