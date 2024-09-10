using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBoatReportType : MonoBehaviour
{
    [SerializeField] private BoatReportType type;
    [SerializeField] private Toggle _toggle;

    private void Awake()
    {
        //_toggle.onValueChanged.AddListener()
    }
}
