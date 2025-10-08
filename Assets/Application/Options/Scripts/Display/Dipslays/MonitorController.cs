using LightHouse.Game.Options;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class MonitorsController : IOptionController
{
    private readonly OptionEnum _widget;
    private readonly MonitorSetting _setting;
    private readonly LocalizedString _displayName;

    public MonitorsController(OptionEnum widget, MonitorSetting setting, LocalizedString displayName)
    {
        _widget = widget; _setting = setting; _displayName = displayName;
    }

    public void Initialize()
    {
        var lis = new List<DisplayInfo>(); Screen.GetDisplayLayout(lis);

        var labels = new List<string>();
        string baseName = _displayName?.GetLocalizedString() ?? "Display";
        for (int i = 0; i < lis.Count; i++) labels.Add($"{baseName} {i + 1} ({lis[i].width}x{lis[i].height})");
        if (labels.Count == 0) labels.Add($"{baseName} 1 ({Screen.currentResolution.width}x{Screen.currentResolution.height})");

        _widget.Choices = labels; _widget.ForceRebuildUI();

        int current = Mathf.Clamp(MonitorSetting.GetCurrentDisplayIndex(), 0, labels.Count - 1);
        try { _widget.SetSelectedIndex(current); } catch { _widget.CurrentChoiceIndex = current; _widget.ForceRebuildUI(); }
        _setting.SetSelectedIndex(current);

        _widget.OnValueChanged -= OnChanged;
        _widget.OnValueChanged += OnChanged;

        MonitorSetting.OnDisplayChanged -= OnSystemChanged;
        MonitorSetting.OnDisplayChanged += OnSystemChanged;
    }

    public void UpdateLanguage() { /* labels stables ici; rebuild si besoin */ }

    private void OnChanged(int idx) => _setting.SetSelectedIndex(idx);

    private void OnSystemChanged()
    {
        int idx = MonitorSetting.GetCurrentDisplayIndex();
        try { _widget.SetSelectedIndex(idx); } catch { _widget.CurrentChoiceIndex = idx; _widget.ForceRebuildUI(); }
        _setting.SetSelectedIndex(idx);
    }
}
