using UnityEngine;
using System;

public class RadioDial : MonoBehaviour
{
    public float sensitivity = 0.2f;
    public float minValue = 88f;
    public float maxValue = 108f;

    public float CurrentValue { get; private set; } = 90f;

    public event Action<float> OnValueChanged;

    private bool _isDragging;

    public InteractableBase _feedback;
    private void Awake()
    {
        _feedback.OnClickDown += Feedback_OnItemClicked;
        _feedback.OnClickUp += Feedback_OnItemClickedReleased;
    }

    private void Feedback_OnItemClickedReleased()
    {
        _isDragging = false;
    }

    private void Feedback_OnItemClicked()
    {
        _isDragging = true;
    }

    private void OnDestroy()
    {
        _feedback.OnClickDown -= Feedback_OnItemClicked;
        _feedback.OnClickUp -= Feedback_OnItemClickedReleased;
    }

    private void Start()
    {
        OnValueChanged?.Invoke(CurrentValue);
    }

    private void Update()
    {
        if (!_isDragging) return;

        float delta = Input.GetAxis("Mouse X");
        CurrentValue += delta * sensitivity;
        CurrentValue = Mathf.Clamp(CurrentValue, minValue, maxValue);
        OnValueChanged?.Invoke(CurrentValue);

        UpdateVisual();
    }

    public void ForceUpdateValue()
    {
        OnValueChanged?.Invoke(CurrentValue);
    }

    private void UpdateVisual()
    {
        float normalized = Mathf.InverseLerp(minValue, maxValue, CurrentValue);
        float angle = Mathf.Lerp(-135f, 135f, normalized);
        transform.localRotation = Quaternion.Euler(angle, -90f, 90f);
    }
}