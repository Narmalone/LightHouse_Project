using MPUIKIT;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct BeaufortScaleInfo
{
    public string ScaleLevel;
    public string WavesHeight;
    public string WaterDescription;
    public List<MultiLanguage> waterDescriptionLanguages;
}


public class WindController : MonoBehaviour
{
    
    public TextMeshProUGUI HeaderTxt;
    public MPImageBasic HeaderBackground;
    public MPImageBasic BodyBackground;
    public CompassController Compas;

    public TMP_InputField WindInputField;
    public BeaufortScale BeaufortController;

    private void Awake()
    {
        WindInputField.onEndEdit.AddListener(OnWindInputChanged);
        BeaufortController.Slider.onValueChanged.AddListener(OnBeaufortSliderChanged);
    }

    private void OnBeaufortSliderChanged(float arg0)
    {
        float windSpeed = (float)Math.Round(BeaufortController.SliderToBeaufort(arg0), 2);
        WindInputField.text = windSpeed.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture); // Formatage avec 2 décimales max
        WindInputField.text = WindInputField.text.Replace('.', ',');
    }

    private void OnDestroy()
    {
        WindInputField.onEndEdit.RemoveListener(OnWindInputChanged);
    }

    private void OnWindInputChanged(string arg0)
    {
        string normalizedInput = arg0.Replace(',', '.'); // Convertit ',' en '.'
        if (float.TryParse(normalizedInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            // Met à jour le slider et les autres éléments avec la valeur saisie
            BeaufortController.UpdateBeaufortTitle();
            BeaufortController.UpdateSlider(result);
        }
        else
        {
            Debug.LogWarning("Invalid wind speed input: " + arg0);
        }
    }
}
