using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HumidityController : MonoBehaviour
{
    public Slider HumiditySlider;
    public TextMeshProUGUI SelectHumidityTxt;
    public TextMeshProUGUI CurrentHumidityValueTxt;
    public BarController Bar;

    private void Awake()
    {
        HumiditySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void Start()
    {
        UpdateTextBySliderValue();
    }

    private void OnDestroy()
    {
        HumiditySlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float arg0)
    {
        float value = arg0 * 100;
        string valeurArrondie = string.Format("{0:F2}", value);
        CurrentHumidityValueTxt.text = valeurArrondie + "%";
    }

    private void UpdateTextBySliderValue()
    {
        float value = HumiditySlider.value * 100;
        string valeurArrondie = string.Format("{0:F2}", value);
        CurrentHumidityValueTxt.text = valeurArrondie + "%";
    }
}
