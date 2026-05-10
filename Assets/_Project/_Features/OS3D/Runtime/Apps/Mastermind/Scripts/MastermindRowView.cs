using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Represents a single mastermind row.
    /// </summary>
    public class MastermindRowView : MonoBehaviour
    {
        #region Events

        public event Action<int> OnGuessSlotClicked;

        #endregion

        #region Inspector

        [SerializeField]
        private GridLayoutGroup _guessSlotsGrid;

        [SerializeField] private TextMeshProUGUI _rowAttemptText;

        [Header("Guess")]

        [SerializeField]
        private MastermindGuessSlotView _guessSlotPrefab;

        [SerializeField]
        private Transform _guessSlotsContainer;

        [Header("Hints")]

        [SerializeField]
        private MastermindHintSlotView _hintSlotPrefab;

        [SerializeField]
        private Transform _hintSlotsContainer;

        [Header("Runtime")]

        [SerializeField]
        private List<MastermindGuessSlotView> _guessSlots =
            new();

        [SerializeField]
        private List<MastermindHintSlotView> _hintSlots =
            new();

        #endregion

        #region Properties

        public IReadOnlyList<MastermindGuessSlotView>
            GuessSlots => _guessSlots;

        public IReadOnlyList<MastermindHintSlotView>
            HintSlots => _hintSlots;

        #endregion

        #region Public API

        public void Initialize(int codeLength, int rowAttemptIndex)
        {
            _rowAttemptText.text = $"{rowAttemptIndex + 1}.";

            GenerateGuessSlots(codeLength);

            GenerateHintSlots(codeLength);
        }

        /// <summary>
        /// Sets the visual color of a guess slot.
        /// </summary>
        public void SetGuessSlotColor(
            int index,
            Color color)
        {
            if (index < 0 ||
                index >= _guessSlots.Count)
                return;

            _guessSlots[index].SetColor(color);
        }

        /// <summary>
        /// Displays hints on the row.
        /// </summary>
        public void SetHints(
            List<MastermindHintType> hints)
        {
            if (hints == null)
                return;

            for (int i = 0;
                 i < _hintSlots.Count;
                 i++)
            {
                // If we exceed provided hints,
                // fallback to empty.
                MastermindHintType hintType =
                    MastermindHintType.Empty;

                if (i < hints.Count)
                {
                    hintType = hints[i];
                }

                _hintSlots[i]
                    .SetHint(hintType);
            }
        }

        public void SetInteractable(bool value)
        {
            for (int i = 0;
                 i < _guessSlots.Count;
                 i++)
            {
                _guessSlots[i]
                    .SetInteractable(value);
            }
        }

        #endregion

        #region Guess Slots

        private void GenerateGuessSlots(int count)
        {
            ClearGuessSlots();

            for (int i = 0;
                 i < count;
                 i++)
            {
                MastermindGuessSlotView slot =
                    Instantiate(
                        _guessSlotPrefab,
                        _guessSlotsContainer);

                slot.Initialize(i);

                slot.OnClicked +=
                    HandleGuessSlotClicked;

                _guessSlots.Add(slot);
            }

            _guessSlotsGrid.constraint =
                GridLayoutGroup.Constraint
                    .FixedColumnCount;

            _guessSlotsGrid.constraintCount =
                Mathf.CeilToInt(
                    Mathf.Sqrt(count));
        }

        private void HandleGuessSlotClicked(
            int index)
        {
            OnGuessSlotClicked?.Invoke(index);
        }

        private void ClearGuessSlots()
        {
            for (int i = 0;
                 i < _guessSlots.Count;
                 i++)
            {
                if (_guessSlots[i] == null)
                    continue;

                _guessSlots[i].OnClicked -=
                    HandleGuessSlotClicked;

                Destroy(
                    _guessSlots[i].gameObject);
            }

            _guessSlots.Clear();
        }

        #endregion

        #region Hint Slots

        private void GenerateHintSlots(int count)
        {
            ClearHintSlots();

            for (int i = 0;
                 i < count;
                 i++)
            {
                MastermindHintSlotView slot =
                    Instantiate(
                        _hintSlotPrefab,
                        _hintSlotsContainer);

                // Initialize empty state
                slot.SetHint(
                    MastermindHintType.Empty);

                _hintSlots.Add(slot);
            }
        }

        private void ClearHintSlots()
        {
            for (int i = 0;
                 i < _hintSlots.Count;
                 i++)
            {
                if (_hintSlots[i] == null)
                    continue;

                Destroy(
                    _hintSlots[i].gameObject);
            }

            _hintSlots.Clear();
        }

        #endregion
    }
}