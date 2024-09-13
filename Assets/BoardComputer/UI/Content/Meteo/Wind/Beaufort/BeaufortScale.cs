using UnityEngine;
using UnityEngine.UI;

public class BeaufortScale : MonoBehaviour
{
    public RectTransform[] BeaufortScales;

    public float[] beaufortBoundaries = new float[] { 0, 1, 5, 11, 19, 28, 38, 49, 61, 74, 88, 102, 128 };
    public float[] sliderValues = new float[] { 0, 0.0833f, 0.1667f, 0.25f, 0.3333f, 0.4167f, 0.5f, 0.5833f, 0.6667f, 0.75f, 0.8333f, 0.9167f, 1 };
    public Slider Slider;

    public float BeaufortToSlider(float windSpeedKmH)
    {
        // Find the corresponding Beaufort scale value
        for (int i = 0; i < beaufortBoundaries.Length - 1; i++)
        {
            if (windSpeedKmH <= beaufortBoundaries[i + 1])
            {
                float sliderValueInterp = sliderValues[i] + (windSpeedKmH - beaufortBoundaries[i]) / (beaufortBoundaries[i + 1] - beaufortBoundaries[i]) * (sliderValues[i + 1] - sliderValues[i]);
                return sliderValueInterp;
            }
        }

        // If wind speed is above 128 km/h, return 1
        return 1;
    }


}
