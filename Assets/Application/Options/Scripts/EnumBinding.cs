using System;
using System.Collections.Generic;
using UnityEngine;

public class EnumBinding<T> where T : struct, Enum
{
    private readonly OptionEnum _widget;
    private readonly Func<T> _getSelected;
    private readonly Action<T> _setSelected;
    private readonly Func<T, string> _toLabel;
    private readonly List<T> _order;

    public EnumBinding(OptionEnum widget, Func<T> getSelected, Action<T> setSelected, Func<T, string> toLabel, List<T> order)
    {
        _widget = widget; _getSelected = getSelected; _setSelected = setSelected; _toLabel = toLabel; _order = order;
    }

    public void Initialize()
    {
        var labels = new List<string>(_order.Count);
        foreach (var v in _order) labels.Add(_toLabel(v));

        _widget.Choices = labels;
        _widget.ForceRebuildUI();

        int idx = Mathf.Clamp(_order.IndexOf(_getSelected()), 0, _order.Count - 1);
        SafeSetIndex(idx);

        _widget.OnValueChanged -= OnChanged;
        _widget.OnValueChanged += OnChanged;
    }

    public void UpdateLanguage()
    {
        // Rebuild les labels (ordre identique)
        int prev = _widget.CurrentChoiceIndex;
        var labels = new List<string>(_order.Count);
        foreach (var v in _order) labels.Add(_toLabel(v));
        _widget.Choices = labels;
        _widget.CurrentChoiceIndex = Mathf.Clamp(prev, 0, labels.Count - 1);
        _widget.ForceRebuildUI();
    }

    private void OnChanged(int newIdx)
    {
        newIdx = Mathf.Clamp(newIdx, 0, _order.Count - 1);
        _setSelected(_order[newIdx]);
    }

    private void SafeSetIndex(int idx)
    {
        try { _widget.SetSelectedIndex(idx); }
        catch { _widget.CurrentChoiceIndex = idx; _widget.ForceRebuildUI(); }
    }
}
