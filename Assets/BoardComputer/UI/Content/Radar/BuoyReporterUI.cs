using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuoyReporterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textLogoNumber;
    [SerializeField] private TextMeshProUGUI _textIdNumber;
    [SerializeField] private TextMeshProUGUI _textReportedBuoy;
    [SerializeField] private Image _ledReported;
    [SerializeField] private Image _borderLogoNumber;
    [SerializeField] private Button _buttonReport;
    [SerializeField] private Color _colorLedReportedIdle;
    [SerializeField] private Color _colorLedReportedActivated;
    [SerializeField] private string _contentIdle;
    [SerializeField] private string _contentActivated;

    public string _id;

    public Action<string> _reportEvent;

    public void Intialize(string id)
    {
        _id = id;

        var idInt = int.Parse(id);

        _textLogoNumber.text= id;
        _textIdNumber.text = $"{(idInt > 99? "" : "0")}{(idInt > 9 ?"":"0")}{id}";

        _textReportedBuoy.text = _contentIdle;

        _ledReported.color = _colorLedReportedIdle;
        _borderLogoNumber.color = _buttonReport.colors.normalColor;
    }

    public void Report()
    {
        _buttonReport.interactable = false;
        _textReportedBuoy.text = _contentActivated;
        _ledReported.color = _colorLedReportedActivated;
        _textLogoNumber.color = _buttonReport.colors.selectedColor;
        _borderLogoNumber.color = _buttonReport.colors.selectedColor;

        // Raise
        _reportEvent.Invoke(_id);
    }

    public void Idle()
    {
        _buttonReport.interactable = true;
        _textReportedBuoy.text = _contentIdle;
        _textLogoNumber.color = _buttonReport.colors.normalColor;
        _borderLogoNumber.color = _buttonReport.colors.normalColor;
        _ledReported.color = _colorLedReportedIdle;
    }
}
