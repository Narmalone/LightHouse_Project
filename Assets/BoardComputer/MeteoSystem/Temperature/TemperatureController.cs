using MPUIKIT;
using TMPro;
using UnityEngine;

public class TemperatureController : MonoBehaviour
{
    [Header("Temperature Title")]
    public TextMeshProUGUI TemperatureTitleTxt;
    public MPImageBasic TemperatureTitleBackground;

    [Header("BODY")]
    public MPImageBasic BodyBackground;

    //air
    [Header("AIR")]
    public TextMeshProUGUI EnterAirTempTxt;
    public TextMeshProUGUI EnterAirTempCelciusTxt;

    //water
    [Header("Water")]
    public TextMeshProUGUI EnterWaterTempTxt;
    public TextMeshProUGUI EnterWaterTempCelciusTxt;

    [Header("Input Fields")]
    public TMP_InputField AirTempInputField;
    public TMP_InputField WaterTempInputFIeld;
}
