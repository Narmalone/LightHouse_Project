using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadialSlider : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform needleTransform;
    [SerializeField] private TMP_Text valueText;

    [Header("Angle Range")]
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 360f;

    public float value = 0f; // 0 to 1

    public void SetValue(float newValue)
    {
        value = Mathf.Clamp01(newValue);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (fillImage != null)
            fillImage.fillAmount = value;

   /*     if (needleTransform != null)
        {
            float angle = Mathf.Lerp(minAngle, maxAngle, value);
            needleTransform.localEulerAngles = new Vector3(0, 0, angle);
        }

        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value * 100f) + " %";*/
    }

    public float GetValue() => value;
}
