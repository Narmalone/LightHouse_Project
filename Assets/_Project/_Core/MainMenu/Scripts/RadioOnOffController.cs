using UnityEngine;

public class RadioOnOffController : MonoBehaviour
{
    [SerializeField] private InteractableBase _interactableBase;
    [SerializeField] private ToggleButton _toggle;
    [SerializeField] private AudioFeedback _audioFeedback;

    public ToggleButton Toggle => _toggle;

    private void Awake()
    {
        _toggle.Bind(_interactableBase);
        _audioFeedback.Bind(_interactableBase);
    }
}
