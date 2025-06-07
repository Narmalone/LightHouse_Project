using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsTest : MonoBehaviour
{
    public Dropdown shadowDropdown;
    public Toggle fogToggle;
    public Slider fogDensitySlider;

    void Start()
    {
        // Initialisation des valeurs de l'UI
        shadowDropdown.onValueChanged.AddListener(SetShadows);
        fogToggle.onValueChanged.AddListener(SetFog);
        fogDensitySlider.onValueChanged.AddListener(SetFogDensity);
    }

    public void SetShadows(int index)
    {
        switch (index)
        {
            case 0: QualitySettings.shadows = ShadowQuality.Disable; break;
            case 1: QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2: QualitySettings.shadows = ShadowQuality.All; break;
        }
    }

    public void SetFog(bool enabled)
    {
        RenderSettings.fog = enabled;
    }

    public void SetFogDensity(float density)
    {
        RenderSettings.fogDensity = density;
    }
}
