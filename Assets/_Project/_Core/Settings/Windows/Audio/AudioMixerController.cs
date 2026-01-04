using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[AddComponentMenu("Audio/Audio Volume Slider")]
public class AudioVolumeSlider : MonoBehaviour
{
    [Header("Wiring")]
    public AudioMixer mixer;
    [SerializeField] private TextMeshProUGUI _sliderValueText;
    [Tooltip("Nom du param exposé (ex: \"SFX_Volume\" ou \"Music_Volume\")")]
    public string exposedParameter = "SFX_Volume";
    public Slider slider;

    [Header("Prefs (optionnel)")]
    [Tooltip("Clé PlayerPrefs. Vide = utilise exposedParameter")]
    public string prefsKey = "";
    public bool DeleteKeyOnStart = true;
    public bool saveOnChange = true;

    // Bornes classiques pour Unity
    const float MinDb = -80f;
    const float MaxDb = 0f;

    void Reset()
    {
        slider = GetComponent<Slider>();
    }

    void Awake()
    {
        if (!slider) slider = GetComponent<Slider>();
        if (string.IsNullOrEmpty(prefsKey)) prefsKey = exposedParameter;

        if (DeleteKeyOnStart && PlayerPrefs.HasKey(prefsKey))
        {
            PlayerPrefs.DeleteKey(prefsKey);
        }
    }

    private void Start()
    {
        if (slider) slider.onValueChanged.AddListener(OnSliderChanged);
        _sliderValueText.text = Mathf.RoundToInt((slider.value * 100)).ToString() + "%";
    }

    void OnDestroy()
    {
        if (slider) slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
    public bool HasKey() => PlayerPrefs.HasKey(prefsKey);
    public void Load()
    {
        if (PlayerPrefs.HasKey(prefsKey))
        {
            var s = PlayerPrefs.GetFloat(prefsKey, 1f);
            slider.SetValueWithoutNotify(s);
            Apply(s);
        }
        
    }

    void OnSliderChanged(float value)
    {
        Apply(value);
        _sliderValueText.text = Mathf.RoundToInt((value * 100)).ToString() + "%";
        if (saveOnChange) PlayerPrefs.SetFloat(prefsKey, value);
    }

    public void Apply(float linear01)
    {
        if (!mixer) return;
        mixer.SetFloat(exposedParameter, LinearToDb(linear01));
    }

    // --- Conversions ---
    public static float LinearToDb(float linear)
    {
        if (linear <= 0f) return MinDb; // mute propre
        return Mathf.Clamp(20f * Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)), MinDb, MaxDb);
    }

    public static float DbToLinear(float dB)
    {
        if (dB <= MinDb) return 0f;
        return Mathf.Pow(10f, dB / 20f);
    }
}
