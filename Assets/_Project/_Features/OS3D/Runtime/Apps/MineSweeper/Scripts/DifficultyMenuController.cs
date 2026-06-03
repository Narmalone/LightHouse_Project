using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer
{
    /// <summary>
    /// Generic difficulty selection menu.
    /// Does NOT know anything about gameplay configs.
    /// </summary>
    public class DifficultyMenuController :
        MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Called when a difficulty button is clicked.
        /// </summary>
        public event Action<int>
            OnDifficultySelected;

        /// <summary>
        /// Called when a difficulty button is hovered.
        /// </summary>
        public event Action<int, string>
            OnDifficultyHovered;

        public event Action OnDifficultyMenuClosed;

        #endregion

        #region Inspector

        [SerializeField] private Button _closeButton;

        [SerializeField]
        private DifficultyButtonView
            _buttonPrefab;

        [SerializeField]
        private Transform
            _buttonsContainer;

        [SerializeField]
        private List<string>
            _difficultyNames =
                new();

        #endregion

        #region Runtime

        private readonly List<DifficultyButtonView>
            _runtimeButtons =
                new();

        #endregion

        #region Public API

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        private void OnCloseButtonClicked()
        {
            OnDifficultyMenuClosed?.Invoke();
            this.gameObject.SetActive(false);
        }

        public void Initialize()
        {
            GenerateButtons();
            this.gameObject.SetActive(false);
        }

        #endregion

        #region Generation

        private void GenerateButtons()
        {
            ClearButtons();

            for (int i = 0;
                 i < _difficultyNames.Count;
                 i++)
            {
                int index = i;

                DifficultyButtonView button =
                    Instantiate(
                        _buttonPrefab,
                        _buttonsContainer);

                button.Initialize(
                    _difficultyNames[i]);

                button.OnClicked += () =>
                {
                    OnDifficultySelected?.Invoke(index);
                };

                button.OnHovered += () =>
                {
                    OnDifficultyHovered?.Invoke(index, _difficultyNames[index]);
                };

                _runtimeButtons.Add(button);
            }
        }

        private void ClearButtons()
        {
            for (int i = 0;
                 i < _runtimeButtons.Count;
                 i++)
            {
                if (_runtimeButtons[i] == null)
                    continue;

                Destroy(
                    _runtimeButtons[i]
                        .gameObject);
            }

            _runtimeButtons.Clear();
        }

        #endregion
    }
}