using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Computer.Mastermind
{
    /// <summary>
    /// Handles the full board rows generation.
    /// </summary>
    public class MastermindBoardView : MonoBehaviour
    {
        #region Inspector

        [Header("References")]

        [SerializeField]
        private MastermindRowView _rowPrefab;

        [SerializeField]
        private Transform _rowsContainer;

        [Header("Runtime")]

        [SerializeField]
        private List<MastermindRowView> _rows =
            new();

        #endregion

        #region Properties

        public IReadOnlyList<MastermindRowView> Rows =>
            _rows;

        #endregion

        #region Public API

        public void GenerateRows(
            int rowsCount,
            int codeLength)
        {
            ClearRows();

            for (int i = 0; i < rowsCount; i++)
            {
                MastermindRowView row =
                    Instantiate(
                        _rowPrefab,
                        _rowsContainer);

                row.Initialize(codeLength, i);

                _rows.Add(row);
            }
        }

        public MastermindRowView GetRow(int index)
        {
            if (index < 0 || index >= _rows.Count)
                return null;

            return _rows[index];
        }

        #endregion

        #region Internal

        private void ClearRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] == null)
                    continue;

                Destroy(_rows[i].gameObject);
            }

            _rows.Clear();
        }

        #endregion
    }
}