using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LightHouse.Options.V3
{
    public class DisplayModeController : MonoBehaviour, IOption
    {
        [Header("UI")]
        public OptionEnum optionEnum;
        public TMP_Text label; // optionnel

        private FullScreenMode _currentMode;   // dernier état “committé”
        private FullScreenMode _selectedMode;  // état UI

        // Mapping EXPLICITE entre index UI et modes Unity
        private readonly List<FullScreenMode> _order = new()
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.MaximizedWindow,
            FullScreenMode.Windowed
        };

        private readonly List<string> _labels = new()
        {
            "ExclusiveFullScreen",
            "FullScreenWindow",
            "MaximizedWindow",
            "Windowed"
        };

        private void Awake()
        {
            if (!optionEnum)
            {
                Debug.LogError("[DisplayModeController] OptionEnum manquant.");
                enabled = false;
            }
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            optionEnum.Choices = new List<string>(_labels);

            // Seed depuis le système une seule fois
            _currentMode = Screen.fullScreenMode;

            int idx = IndexOfMode(_currentMode);
            if (idx < 0) idx = 0; // fallback
            optionEnum.SetValueWithoutNotify(idx);

            _selectedMode = _order[idx];

            if (label) label.text = "Display Mode";
        }

        public bool HasChanges()
        {
            int idx = Mathf.Clamp(optionEnum.CurrentChoiceIndex, 0, _order.Count - 1);
            _selectedMode = _order[idx];
            return _selectedMode != _currentMode;
        }

        public void Apply()
        {
            int idx = Mathf.Clamp(optionEnum.CurrentChoiceIndex, 0, _order.Count - 1);
            _selectedMode = _order[idx];

            if (_selectedMode == _currentMode) return;

            var r = Screen.currentResolution;
            Screen.SetResolution(r.width, r.height, _selectedMode);

            // Commit logique immédiat
            _currentMode = _selectedMode;
        }

        public void Revert()
        {
            // UI ← current, selon NOTRE mapping (pas l’enum brute)
            int idx = IndexOfMode(_currentMode);
            if (idx < 0) idx = 0;
            optionEnum.SetValueWithoutNotify(idx);
            _selectedMode = _currentMode;
        }

        private int IndexOfMode(FullScreenMode mode)
        {
            for (int i = 0; i < _order.Count; i++)
                if (_order[i] == mode) return i;
            return -1;
        }

        // Debug overlay (build ok)
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 420, 20), "[DisplayModeController]");
            GUI.Label(new Rect(10, 30, 420, 20), $"Current:  {_currentMode}");
            GUI.Label(new Rect(10, 50, 420, 20), $"Selected: {_selectedMode}");
            GUI.Label(new Rect(10, 70, 420, 20), $"System:   {Screen.fullScreenMode}");
            GUI.Label(new Rect(10, 90, 420, 20), $"UI Index: {optionEnum.CurrentChoiceIndex}");
        }
    }
}
