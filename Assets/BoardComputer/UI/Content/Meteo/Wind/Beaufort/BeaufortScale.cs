using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeaufortScale : MonoBehaviour
{
    public RectTransform[] BeaufortScales;

    private float[] _beaufortBoundaries = new float[] { 0, 1, 5, 11, 19, 28, 38, 49, 61, 74, 88, 102, 128 };
    private float[] _sliderValues = new float[] { 0, 0.0833f, 0.1667f, 0.25f, 0.3333f, 0.4167f, 0.5f, 0.5833f, 0.6667f, 0.75f, 0.8333f, 0.9167f, 1 };
    public List<BeaufortScaleInfo> _beaufortScaleInfo = new List<BeaufortScaleInfo>();

    public Slider Slider;
    public TextMeshProUGUI ScaleTitleText;
    public TextMeshProUGUI ScaleValueText;    
    
    public TextMeshProUGUI DescriptionTitleText;
    public TextMeshProUGUI DescriptionValueText;

    public TextMeshProUGUI WaveHeightTitleText;
    public TextMeshProUGUI WaveHeightValueText;

    public int CurrentBeaufortIndex = -1;

    public void UpdateBeaufort(BeaufortScaleInfo scaleInfo)
    {
        ScaleValueText.text = scaleInfo.ScaleLevel;
        DescriptionValueText.text = scaleInfo.WaterDescription;
        WaveHeightValueText.text = scaleInfo.WavesHeight;
    }

    public void UpdateSlider(float windSpeedKmH)
    {
        Slider.value = Mathf.Clamp(BeaufortToSlider(windSpeedKmH), 0, 1);
    }


    public void UpdateBeaufortTitle()
    {
        //DescriptionValueText.text = CurrentBeaufortIndex.ToString();
    }

    public float BeaufortToSlider(float windSpeedKmH)
    {
        // Find the corresponding Beaufort scale value
        for (int i = 0; i < _beaufortBoundaries.Length - 1; i++)
        {
            if (windSpeedKmH <= _beaufortBoundaries[i + 1])
            {
                float sliderValueInterp = _sliderValues[i] + (windSpeedKmH - _beaufortBoundaries[i]) / (_beaufortBoundaries[i + 1] - _beaufortBoundaries[i]) * (_sliderValues[i + 1] - _sliderValues[i]);
                CurrentBeaufortIndex = i;
                return sliderValueInterp;
            }
        }

        CurrentBeaufortIndex = _beaufortBoundaries.Length - 1;
        // If wind speed is above 128 km/h, return 1
        return 1;
    }

    public float SliderToBeaufort(float sliderValue)
    {
        // Parcourir les plages de slider pour trouver la correspondance
        for (int i = 0; i < _sliderValues.Length - 1; i++)
        {
            if (sliderValue <= _sliderValues[i + 1])
            {
                // Interpolation inverse pour retrouver la vitesse du vent
                float windSpeedInterp = _beaufortBoundaries[i] +
                    (sliderValue - _sliderValues[i]) / (_sliderValues[i + 1] - _sliderValues[i])
                    * (_beaufortBoundaries[i + 1] - _beaufortBoundaries[i]);
                return windSpeedInterp;
            }
        }

        // Si le slider est au maximum, retourner la vitesse maximale
        return _beaufortBoundaries[_beaufortBoundaries.Length - 1];
    }


}
