using TMPro;
using UnityEngine;

public class HumidityRateController : MonoBehaviour
{
    [SerializeField] private RectTransform needleTransform;     // pivot de l'aiguille
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private ClickableCircleSlider _radialRingSlider;
    [SerializeField] private float needleRotationOffset = -180f;

    [SerializeField] private UILineRendererWithoutGrid _customLineRenderer;

    private void Awake()
    {
        _radialRingSlider.OnValueChanged += RadialSlider_OnValueChanged;
    }

    private void OnDestroy()
    {
        _radialRingSlider.OnValueChanged -= RadialSlider_OnValueChanged;
    }

    private void RadialSlider_OnValueChanged(float newValue)
    {
        UpdateHumiditySlider(newValue);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SendDatasToGraph(_radialRingSlider.CurrentValue);
        }
    }

    private void UpdateHumiditySlider(float value)
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

    private void SendDatasToGraph(float humidityValue)
    {
        const float xSpacing = 65f;

        // Y = normalisé en fonction de la hauteur du graphe
        RectTransform rt = _customLineRenderer.rectTransform;
        float graphHeight = rt.rect.height;
        float y = Mathf.Lerp(0, graphHeight, humidityValue);

        // X = index * spacing (0 pour le plus vieux, max 7 * 65 = 455)
        int pointCount = Mathf.Min(_customLineRenderer.points.Count, 8);
        float x = pointCount * xSpacing;

        AddPoint(new Vector2(x, y));
    }

    public void RecalculateX(float spacing)
    {
        for (int i = 0; i < _customLineRenderer.points.Count; i++)
        {
            Vector2 p = _customLineRenderer.points[i];
            _customLineRenderer.points[i] = new Vector2(i * spacing, p.y);
        }

        _customLineRenderer.SetVerticesDirty();
    }


    public void AddPoint(Vector2 newPoint)
    {
        if (_customLineRenderer.points.Count >= 8)
            _customLineRenderer.points.RemoveAt(0);

        _customLineRenderer.points.Add(newPoint);
        RecalculateX(65f); // <- Appelé ici
    }


}
