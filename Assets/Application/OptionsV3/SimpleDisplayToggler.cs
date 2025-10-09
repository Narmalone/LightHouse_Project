using System.Collections.Generic;
using UnityEngine;

public class SimpleDisplayModeToggler : MonoBehaviour
{
    [Header("Keys")]
    public KeyCode nextKey = KeyCode.LeftArrow;     // "suivant"
    public KeyCode prevKey = KeyCode.RightArrow;    // "précédent"
    public KeyCode applyKey = KeyCode.Return;        // ENTER = Apply
    public KeyCode revertKey = KeyCode.Backspace;     // BACKSPACE = Revert

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

    // état logique
    private FullScreenMode _currentMode;  // dernier état appliqué (committé)
    private int _selectedIndex;           // sélection en cours (carousel)

    void Start()
    {
        // seed depuis le système (une fois)
        _currentMode = Screen.fullScreenMode;
        int idx = IndexOfMode(_currentMode);
        if (idx < 0) idx = 0;
        _selectedIndex = idx;
        Log($"Start → current={_currentMode}, selected={_labels[_selectedIndex]}");
    }

    void Update()
    {
        if (Input.GetKeyDown(nextKey)) SelectNext();
        if (Input.GetKeyDown(prevKey)) SelectPrev();
        if (Input.GetKeyDown(applyKey)) Apply();
        if (Input.GetKeyDown(revertKey)) Revert();
    }

    private void SelectNext()
    {
        _selectedIndex = (_selectedIndex + 1) % _order.Count;
        Log($"SelectNext → selected={_labels[_selectedIndex]}");
    }

    private void SelectPrev()
    {
        _selectedIndex = (_selectedIndex - 1 + _order.Count) % _order.Count;
        Log($"SelectPrev → selected={_labels[_selectedIndex]}");
    }

    private void Apply()
    {
        var desired = _order[_selectedIndex];
        if (desired == _currentMode) { Log("Apply → rien à faire"); return; }

        var r = Screen.currentResolution;
        Screen.SetResolution(r.width, r.height, desired);
        _currentMode = desired;

        Log($"Apply → system={Screen.fullScreenMode}, current={_currentMode}");
    }

    private void Revert()
    {
        // remet juste la sélection sur le dernier "committé"
        int idx = IndexOfMode(_currentMode);
        if (idx < 0) idx = 0;
        _selectedIndex = idx;

        Log($"Revert → selected={_labels[_selectedIndex]} (current={_currentMode})");
    }

    private int IndexOfMode(FullScreenMode mode)
    {
        for (int i = 0; i < _order.Count; i++)
            if (_order[i] == mode) return i;
        return -1;
    }

    private void Log(string msg) => Debug.Log($"[SimpleDisplayModeToggler] {msg}");

    // Debug overlay visible en build (enlève si tu veux pas)
    void OnGUI()
    {
        const int w = 480;
        GUI.Label(new Rect(10, 10, w, 20), "[SimpleDisplayModeToggler]");
        GUI.Label(new Rect(10, 30, w, 20), $"Current:   {_currentMode}");
        GUI.Label(new Rect(10, 50, w, 20), $"Selected:  {_labels[_selectedIndex]}");
        GUI.Label(new Rect(10, 70, w, 20), $"System:    {Screen.fullScreenMode}");
        GUI.Label(new Rect(10, 90, w, 20), $"Keys: ← next, → prev, Enter apply, Backspace revert");
    }
}
