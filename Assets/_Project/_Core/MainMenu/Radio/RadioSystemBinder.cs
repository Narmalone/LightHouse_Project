using UnityEngine;

public class RadioSystemBinder : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private InteractableBase interactable;
    [SerializeField] private RadioDial dial;
    [SerializeField] private RadioFrequencyController frequencyController;

    [Header("Feedback")]
    [SerializeField] private AudioFeedback audioFeedback;
    [SerializeField] private EmissiveFeedback emissiveFeedback;

    [SerializeField] private bool _enableActiveState = false;
    private bool _isActive;
    private bool _isEnabled;

    private void Awake()
    {
        // Bind logique métier
        if (dial != null && frequencyController != null)
            dial.OnValueChanged += frequencyController.SetFrequency;

        // Bind feedback audio
        if (audioFeedback != null)
            audioFeedback.Bind(interactable);

        // Bind interaction
        interactable.OnHoverEnter += OnHoverEnter;
        interactable.OnHoverExit += OnHoverExit;
        interactable.OnClickDown += OnClick;
    }

    private void OnDestroy()
    {
        if (dial != null && frequencyController != null)
            dial.OnValueChanged -= frequencyController.SetFrequency;

        interactable.OnHoverEnter -= OnHoverEnter;
        interactable.OnHoverExit -= OnHoverExit;
        interactable.OnClickDown -= OnClick;
    }

    // ─────────────────────────────
    // INTERACTION
    // ─────────────────────────────

    private void OnHoverEnter()
    {
        if (!_isEnabled) return;

        emissiveFeedback.ApplyState(EmissiveState.Hover);
    }

    private void OnHoverExit()
    {
        if (!_isEnabled) return;

        emissiveFeedback.ApplyState(_isActive
            ? EmissiveState.Active
            : EmissiveState.Default);
    }

    private void OnClick()
    {
        if (!_isEnabled || !_enableActiveState) return;

        _isActive = !_isActive;

        emissiveFeedback.ApplyState(_isActive
            ? EmissiveState.Active
            : EmissiveState.Default);
    }

    // ─────────────────────────────
    // EXTERNAL CONTROL (Radio ON/OFF)
    // ─────────────────────────────

    public void SetEnable(bool enable)
    {
        _isEnabled = enable;
        audioFeedback.SetEnable(enable);

        if (!_isEnabled)
        {
            emissiveFeedback.ApplyState(EmissiveState.Disabled);
            return;
        }

        emissiveFeedback.ApplyState(_isActive
            ? EmissiveState.Active
            : EmissiveState.Default);
    }
}