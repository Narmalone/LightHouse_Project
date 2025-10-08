using LightHouse.Game.Options;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MonitorSetting : IOptionSetting
{
    public static event Action OnDisplayChanged;

    private int _current, _selected, _backup;
    private Vector2Int _backupRes;
    private FullScreenMode _backupMode;
    private bool _inFlight;

    public int SelectedIndex => _selected;

    public MonitorSetting()
    {
        _current = GetCurrentDisplayIndex();
        _selected = _current;
        _backup = _current;
    }

    public void SetSelectedIndex(int idx) => _selected = idx;
    public bool HasChanged() => _selected != _current;

    public void Apply()
    {
        if (_inFlight || !HasChanged()) return;

        var layout = new List<DisplayInfo>(); Screen.GetDisplayLayout(layout);
        if (layout.Count == 0) return;

        _selected = Mathf.Clamp(_selected, 0, layout.Count - 1);

        _backup = _current;
        _backupRes = new Vector2Int(ResolutionSetting.CurrentResolution.x, ResolutionSetting.CurrentResolution.y);
        _backupMode = Screen.fullScreenMode;

        var target = layout[_selected];
        _inFlight = true;
        var op = Screen.MoveMainWindowTo(in target, Vector2Int.zero);
        op.completed += _ =>
        {
            int w = Mathf.Min(_backupRes.x, target.width);
            int h = Mathf.Min(_backupRes.y, target.height);
            Screen.SetResolution(w, h, _backupMode);
            _current = _selected;
            _inFlight = false;
            OnDisplayChanged?.Invoke();
        };
    }

    public void Revert()
    {
        if (_inFlight) return;

        var layout = new List<DisplayInfo>(); Screen.GetDisplayLayout(layout);
        if (layout.Count == 0) return;

        _backup = Mathf.Clamp(_backup, 0, layout.Count - 1);

        _inFlight = true;
        var op = Screen.MoveMainWindowTo(layout[_backup], Vector2Int.zero);
        op.completed += _ =>
        {
            Screen.SetResolution(_backupRes.x, _backupRes.y, _backupMode);
            _selected = _current = _backup;
            _inFlight = false;
            OnDisplayChanged?.Invoke();
        };
    }

    // utils
    public static int GetCurrentDisplayIndex()
    {
        var layout = new List<DisplayInfo>(); Screen.GetDisplayLayout(layout);
        var cur = Screen.mainWindowDisplayInfo;
        for (int i = 0; i < layout.Count; i++) if (layout[i].name == cur.name) return i;
        return 0;
    }
}
