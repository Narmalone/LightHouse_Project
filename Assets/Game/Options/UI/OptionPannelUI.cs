using UnityEngine;
using UnityEngine.UIElements;

public class OptionsPanelUI : MonoBehaviour
{
    private SliderInt volumeSlider;
    private DropdownField resolutionDropdown;
    private Toggle fullscreenToggle;
    private Button applyButton;
    private Button backButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        volumeSlider = root.Q<SliderInt>("VolumeSlider");
        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        applyButton = root.Q<Button>("ApplyButton");
        backButton = root.Q<Button>("BackButton");

        applyButton.clicked += ApplyOptions;
        backButton.clicked += BackToMainMenu;
    }

    private void ApplyOptions()
    {
        Debug.Log("Volume: " + volumeSlider.value);
        Debug.Log("Résolution: " + resolutionDropdown.value);
        Debug.Log("Plein écran: " + fullscreenToggle.value);

        // Exemple d'application (résolution)
        string[] res = resolutionDropdown.value.Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, fullscreenToggle.value);

        // Exemple volume (il faut lier à ton AudioMixer)
        // AudioManager.Instance.SetVolume(volumeSlider.value / 100f);
    }

    private void BackToMainMenu()
    {
        Debug.Log("Retour au menu principal !");
        // ici tu peux cacher ce panneau ou charger une autre scène par exemple
    }
}
