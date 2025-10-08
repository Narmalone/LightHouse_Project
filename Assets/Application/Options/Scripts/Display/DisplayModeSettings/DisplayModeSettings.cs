using LightHouse.Game.Options;
using UnityEngine;

public enum DisplayModeOption { Fullscreen, Borderless, MaximizedWindow, Windowed }

public class DisplayModeSetting : IOptionSetting
{
    private DisplayModeOption _current, _selected, _backup;

    public DisplayModeSetting()
    {
        _current = FromUnity(Screen.fullScreenMode);
        _selected = _current;
        _backup = _current;
    }

    public void SetSelected(DisplayModeOption opt) => _selected = opt;
    public bool HasChanged() => _selected != _current;

    public void Apply()
    {
        if (!HasChanged()) return;
        _backup = _current;
        var m = ToUnity(_selected);
        var r = ResolutionSetting.CurrentResolution;
        Screen.SetResolution(r.x, r.y, m);
        _current = _selected;
    }

    public void Revert()
    {
        var m = ToUnity(_backup);
        var r = ResolutionSetting.CurrentResolution;
        Screen.SetResolution(r.x, r.y, m);
        _selected = _current = _backup;
    }

    private static DisplayModeOption FromUnity(FullScreenMode m) => m switch
    {
        FullScreenMode.ExclusiveFullScreen => DisplayModeOption.Fullscreen,
        FullScreenMode.FullScreenWindow => DisplayModeOption.Borderless,
        FullScreenMode.MaximizedWindow => DisplayModeOption.MaximizedWindow,
        _ => DisplayModeOption.Windowed
    };

    private static FullScreenMode ToUnity(DisplayModeOption m) => m switch
    {
        DisplayModeOption.Fullscreen => FullScreenMode.ExclusiveFullScreen,
        DisplayModeOption.Borderless => FullScreenMode.FullScreenWindow,
        DisplayModeOption.MaximizedWindow => FullScreenMode.MaximizedWindow,
        _ => FullScreenMode.Windowed
    };
}
