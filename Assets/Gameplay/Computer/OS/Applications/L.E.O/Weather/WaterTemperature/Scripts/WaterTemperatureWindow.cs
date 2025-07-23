using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterTemperatureWindow : MonoBehaviour
{
    [SerializeField] private TMP_InputField IPF_Temperature;
    [SerializeField] private Material _airThermometerShader;
    [SerializeField] private Image _thermometerImage;
    [SerializeField] private TextMeshProUGUI _tipTitleText;
    [SerializeField] private TextMeshProUGUI _tipDescriptionText;
    [SerializeField] private float _minTemp = -10f;
    [SerializeField] private float _maxTemp = 30f;
    public TemperatureTips[] Tips;

    private void Awake()
    {
        IPF_Temperature.onValueChanged.AddListener(OnTemperatureChanged);
    }

    private void OnDestroy()
    {
        IPF_Temperature.onValueChanged.RemoveListener(OnTemperatureChanged);
    }

    private void OnTemperatureChanged(string arg0)
    {
        if (float.TryParse(arg0, out float temperature))
        {
            float thermometerHeight = _thermometerImage.rectTransform.rect.height;
            float thermoFill = Mathf.InverseLerp(_minTemp, _maxTemp, temperature);
            _airThermometerShader.SetFloat("_FillAmount", thermoFill);

            TemperatureTips tip = GetTipByTemperature(temperature);

            _tipTitleText.text = tip.Title;
            _tipDescriptionText.text = tip.Description;
        }
        else
        {
            Debug.LogWarning($"Valeur non valide : {arg0}");
            _airThermometerShader.SetFloat("_FillAmount", 1f);
        }
    }

    private TemperatureTips GetTipByTemperature(float temperature)
    {
        TemperatureTips selectedTip = new TemperatureTips();
        foreach (var t in Tips)
        {
            if (t.CanBeSelected(temperature))
            {
                selectedTip = t;
            }
        }
        return selectedTip;
    }
}
