using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownBinding
{
    private readonly TMP_Dropdown _dd;
    private readonly Func<int> _getIndex;
    private readonly Action<int> _setIndex;
    private readonly Func<List<string>> _getLabels;

    public DropdownBinding(TMP_Dropdown dd, Func<int> getIndex, Action<int> setIndex, Func<List<string>> getLabels)
    {
        _dd = dd; _getIndex = getIndex; _setIndex = setIndex; _getLabels = getLabels;
    }

    public void Initialize()
    {
        Build();
        _dd.onValueChanged.RemoveListener(OnChanged);
        _dd.value = Mathf.Clamp(_getIndex(), 0, Mathf.Max(0, _dd.options.Count - 1));
        _dd.RefreshShownValue();
        _dd.onValueChanged.AddListener(OnChanged);
        _setIndex(_dd.value); // push modèle
    }

    public void UpdateLanguage() => Build();

    private void Build()
    {
        var labels = _getLabels();
        var opts = new List<TMP_Dropdown.OptionData>(labels.Count);
        foreach (var s in labels) opts.Add(new TMP_Dropdown.OptionData(s));
        _dd.options = opts;
    }

    private void OnChanged(int idx) => _setIndex(idx);
}
