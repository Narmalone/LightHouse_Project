using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuoyReporterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textLogoNumber;
    [SerializeField] private TextMeshProUGUI _textIdNumber;
    [SerializeField] private TextMeshProUGUI _textReportedBuoy;
    [SerializeField] private Image _ledReported;
    [SerializeField] private Image _backgroundBuntonReported;
    [SerializeField] private Image _borderLogoNumber;
    [SerializeField] private Color _colorLedReportedIdle;
    [SerializeField] private Color _colorLedReportedActivated;
    [SerializeField] private Color _colorReportIdle;
    [SerializeField] private Color _colorReportActivated;
    [SerializeField] private string _contentIdle;
    [SerializeField] private string _contentActivated;

    private int _id;

    public void Intialize(int id)
    {
        _id = id;

        var idString = id.ToString();

        _textLogoNumber.text= idString;
        _textIdNumber.text = $"{(id > 99? "" : "0")}{(id > 9 ?"":"0")}{idString}";

        _textReportedBuoy.text = _contentIdle;

        _ledReported.color = _colorLedReportedIdle;
        _backgroundBuntonReported.color = _colorReportIdle;
        _borderLogoNumber.color = _colorReportIdle;
    }
}
