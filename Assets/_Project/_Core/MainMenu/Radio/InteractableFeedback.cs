using UnityEngine;
using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using System;

[RequireComponent(typeof(Renderer))]
public class InteractableFeedback : MonoBehaviour, IRaycastable
{
    public event Action OnRaycastEntered, OnRaycastExited, OnItemClicked, OnItemClickedReleased;
    [Header("Emissive")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string emissiveProperty = "_EmissionColor";
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private Color hoverColor = Color.green;
    [SerializeField] private float baseIntensity = 5f;
    [SerializeField] private float hoverIntensity = 10f;

    [Header("Audio")]
    [SerializeField] private AudioCue hoverSound;
    [SerializeField] private AudioCue tickSound;

    [Header("Physical Feedback")]
    [SerializeField] private Vector3 hoverOffset = new Vector3(0, -0.002f, 0);
    [SerializeField] private float moveSpeed = 10f;

    private MaterialPropertyBlock _mpb;
    private Vector3 _initialLocalPos;
    private Vector3 _targetLocalPos;

    private bool _isHovered;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        _mpb = new MaterialPropertyBlock();

        _initialLocalPos = transform.localPosition;
        _targetLocalPos = _initialLocalPos;

        ApplyEmissive(baseIntensity, baseColor);
    }

    private void Update()
    {
        // Smooth movement
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _targetLocalPos,
            Time.deltaTime * moveSpeed
        );
    }

    // ─────────────────────────────────────────────
    // IRaycastable
    // ─────────────────────────────────────────────

    public void OnRaycastEnter()
    {
        if (_isHovered) return;
        _isHovered = true;

        // Emissive boost
        ApplyEmissive(hoverIntensity, hoverColor);

        // Audio
        if (hoverSound != null)
        {
            ServiceLocator.Audio.PlayAt(hoverSound, transform.position);
        }

        // Physical feedback
        _targetLocalPos = _initialLocalPos + hoverOffset;
        OnRaycastEntered?.Invoke();
    }

    public void OnRaycastLeave()
    {
        if (!_isHovered) return;
        _isHovered = false;

        // Reset emissive
        ApplyEmissive(baseIntensity, baseColor);

        // Reset position
        _targetLocalPos = _initialLocalPos;
        OnRaycastExited?.Invoke();
    }

    // ─────────────────────────────────────────────
    private const string EmissiveColorID = "_EmissiveColor";
    private void ApplyEmissive(float intensity, Color targetColor)
    {
        targetRenderer.GetPropertyBlock(_mpb);

        _mpb.SetColor(EmissiveColorID, targetColor * intensity);

        targetRenderer.SetPropertyBlock(_mpb);
    }

    public void OnClicked()
    {
        if (tickSound != null)
        {
            ServiceLocator.Audio.PlayAt(tickSound, transform.position);
        }
        OnItemClicked?.Invoke();
    }

    public void OnClickReleased() { OnItemClickedReleased?.Invoke(); }
}