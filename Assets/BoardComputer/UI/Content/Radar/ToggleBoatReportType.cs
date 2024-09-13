using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBoatReportType : MonoBehaviour
{
    [SerializeField] private BoatReportType type;
    [SerializeField] private Toggle _toggle;

    public Action<BoatReportType> _updateType;

    private void Awake()
    {
        _toggle.onValueChanged.AddListener(RaiseEvent);
    }

    private void RaiseEvent(bool b)
    {
        _updateType.Invoke(b ? type : BoatReportType.NULL);
    }
}
