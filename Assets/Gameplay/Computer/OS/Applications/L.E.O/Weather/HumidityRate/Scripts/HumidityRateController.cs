using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class HumidityRateController : MonoBehaviour
{
    [SerializeField] private RectTransform needleTransform;     // pivot de l'aiguille
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private ClickableCircleSlider _radialRingSlider;
    [SerializeField] private float needleRotationOffset = -180f;

    private void Awake()
    {
        _radialRingSlider.OnValueChanged += _radialRingSlider_OnValueChanged;
    }

    private void OnDestroy()
    {
        _radialRingSlider.OnValueChanged -= _radialRingSlider_OnValueChanged;
    }

    private void _radialRingSlider_OnValueChanged(float newValue)
    {
        UpdateUI(newValue);
    }

    private void UpdateUI(float value)
    {
        valueText.text = Mathf.RoundToInt(value * 100f) + " %";

        // Direction de l'aiguille = depuis son pivot vers le point extérieur cliqué
        Vector3 pivotWorldPos = needleTransform.position;
        Vector3 targetWorldPos = _radialRingSlider.WorldOuterPoint;

        Vector3 direction = targetWorldPos - pivotWorldPos;

        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Tourne l’aiguille vers la direction, en ajoutant un offset si le sprite n'est pas orienté vers la droite
        needleTransform.localEulerAngles = new Vector3(0f, 0f, angleZ + needleRotationOffset);
    }
}
