using MPUIKIT;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct BeaufortScaleInfo
{
    public string Scale;
    public string WavesHeight;
    public string Description;

    public BeaufortScaleInfo(string scale, string wavesHeight, string description)
    {
        this.Scale = scale;
        this.WavesHeight = wavesHeight;
        this.Description = description;
    }

    public void SetDescription()
    {

    }
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
        WindInputField.onValueChanged.AddListener(OnWindInputChanged);
    }

    private void OnDestroy()
    {
        WindInputField.onValueChanged.RemoveListener(OnWindInputChanged);
    }

    private void OnWindInputChanged(string arg0)
    {
        int.TryParse(arg0, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int result);
        BeaufortController.UpdateBeaufortTitle();
        BeaufortController.UpdateSlider(result);
        
    }
}
