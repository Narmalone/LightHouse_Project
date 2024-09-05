using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeteoContent : ContentWindow
{
    [SerializeField] private TextMeshProUGUI _weatherReportTxt;   
    public TemperatureController TemperatureController;
    public QualityAtmosphereController QualityAtmosphereController;

}
