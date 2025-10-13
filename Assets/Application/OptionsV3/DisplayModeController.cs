using LightHouse.Options.V3;
using System.Collections.Generic;
using UnityEngine;

public class DisplayModeController : MonoBehaviour, IOption
{
    [Header("UI (passive)")]
    public OptionEnumPassive optionEnum;  // assigne dans l’inspector

    [Header("Keys")]
    public KeyCode nextKey = KeyCode.LeftArrow;
    public KeyCode prevKey = KeyCode.RightArrow;
    public KeyCode applyKey = KeyCode.Return;
    public KeyCode revertKey = KeyCode.Backspace;

    [Header("Order & Labels (UI/debug)")]
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

    // état logique unique
    private FullScreenMode _currentMode;
    private int _selectedIndex;

    void Awake()
    {
        if (!optionEnum)
        {
            Debug.LogError("[SimpleDisplayModeTogglerWithUI] OptionEnumPassive manquant.");
            enabled = false;
            return;
        }

        // branchement des clics UI -> nos fonctions (pas d'état côté UI)
        optionEnum.OnPrevClicked += SelectPrev;
        optionEnum.OnNextClicked += SelectNext;

        // pousse les labels dans l'UI
        optionEnum.SetChoices(new List<string>(_labels));
    }

    void Start()
    {
        // seed depuis le système (une fois)
        _currentMode = Screen.fullScreenMode;
        int idx = IndexOfMode(_currentMode);
        if (idx < 0) idx = 0;
        _selectedIndex = idx;

        // affiche l’index courant dans l’UI
        optionEnum.ShowIndex(_selectedIndex);

        Log($"Start → current={_currentMode}, selected={_labels[_selectedIndex]}");
    }

    // --- logique coeur ---

    private void SelectNext()
    {
        _selectedIndex = (_selectedIndex + 1) % _order.Count;
        optionEnum.ShowIndex(_selectedIndex); // UI suit notre index
        Log($"SelectNext → selected={_labels[_selectedIndex]}");
    }

    private void SelectPrev()
    {
        _selectedIndex = (_selectedIndex - 1 + _order.Count) % _order.Count;
        optionEnum.ShowIndex(_selectedIndex);
        Log($"SelectPrev → selected={_labels[_selectedIndex]}");
    }
    public FullScreenMode CurrentCommittedMode => _currentMode;
    public event System.Action<FullScreenMode> OnModeCommitted;
    public void Apply()
    {
        if (!HasChanges()) return;
        var desired = _order[_selectedIndex];
        if (desired == _currentMode) { Log("Apply → rien à faire"); return; }

        var r = Screen.currentResolution;
        Screen.SetResolution(r.width, r.height, desired);
        _currentMode = desired;
        OnModeCommitted?.Invoke(_currentMode); // notifie les autres

        Log($"Apply → system={Screen.fullScreenMode}, current={_currentMode}");
    }

    public void Revert()
    {
        if (!HasChanges()) return;
        // remet la sélection sur le dernier committé
        int idx = IndexOfMode(_currentMode);
        if (idx < 0) idx = 0;
        _selectedIndex = idx;

        optionEnum.ShowIndex(_selectedIndex);

        Log($"Revert → selected={_labels[_selectedIndex]} (current={_currentMode})");
    }

    // --- utils & debug ---

    private int IndexOfMode(FullScreenMode mode)
    {
        for (int i = 0; i < _order.Count; i++)
            if (_order[i] == mode) return i;
        return -1;
    }

    private void Log(string msg) => Debug.Log($"[SimpleDisplayModeTogglerWithUI] {msg}");

    public bool HasChanges()
    {
        // "Apply" ferait quelque chose si et seulement si la sélection UI
        // n'est pas égale à l'état committé (_currentMode).
        if (_order.Count == 0) return false;

        int clamped = Mathf.Clamp(_selectedIndex, 0, _order.Count - 1);
        var desired = _order[clamped];
        return desired != _currentMode;
    }

}
