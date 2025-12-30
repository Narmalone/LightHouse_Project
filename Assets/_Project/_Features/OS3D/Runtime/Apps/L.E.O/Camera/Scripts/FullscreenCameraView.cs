using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenCameraView : MonoBehaviour
{
    public event Action OnFullScreenButtonCliqued;
    [SerializeField] private RawImage _renderTexture;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private Button _fullscreenButton;

    public TextMeshProUGUI DayText => _dayText;

    private void Awake()
    {
        _fullscreenButton.onClick.AddListener(OnFullScreenCliqued);
    }

    private void OnDestroy()
    {
        _fullscreenButton.onClick.RemoveListener(OnFullScreenCliqued);
    }

    private void OnFullScreenCliqued()
    {
        OnFullScreenButtonCliqued?.Invoke();
    }

    public void SetTexture(RenderTexture texture)
    {
        _renderTexture.texture = texture;
    }
}
