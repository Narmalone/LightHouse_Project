using System;
using UnityEngine;

public class ElectricDurabilityController : MonoBehaviour
{
    public event Action OnDurabilityEnded;

    public float MaxDurabilityItem { get; private set; }
    public float CurrentDurability { get; private set; }
    private bool isDurabilityActive = true;

    [SerializeField] private MeshRenderer _electricItemRenderer;
    [SerializeField] private float _maxDurability = 60f;
    [SerializeField] private float _emissionIntensity = 1f;

    [SerializeField] private Color _fullDurabilityColor = Color.green;
    [SerializeField] private Color _durabilityDepletedColor = Color.red;

    private Material _materialInstance;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissiveColor");

    private void Awake()
    {
        MaxDurabilityItem = _maxDurability;
        CurrentDurability = _maxDurability;

        // Instancie le material pour ne pas modifier l'asset partagé
        _materialInstance = _electricItemRenderer.material;
        _materialInstance.EnableKeyword("_EMISSION");

        SetMaterialColorToOff();
    }

    private void Update()
    {
        if (!isDurabilityActive) return;

        CurrentDurability -= Time.deltaTime;
        if (CurrentDurability <= 0)
        {
            CurrentDurability = 0;
            isDurabilityActive = false;
            OnDurabilityEnded?.Invoke();
            // Handle durability reaching zero (e.g., disable the electric item)
        }

        UpdateMaterialColor();
    }

    public void SetActiveDurability(bool value)
    {
        isDurabilityActive = value;

        if (!value)
        {
            SetMaterialColorToOff();
        }
    }

    private void SetMaterialColorToOff()
    {
        _materialInstance.SetColor(EmissionColorId, Color.gray);
    }

    public void SetDurability(float durability, float maxDurability)
    {
        CurrentDurability = durability;
        MaxDurabilityItem = maxDurability;
        UpdateMaterialColor();
    }

    private void UpdateMaterialColor()
    {
        float ratio = MaxDurabilityItem > 0 ? CurrentDurability / MaxDurabilityItem : 0f;
        Color emissionColor = Color.Lerp(_durabilityDepletedColor, _fullDurabilityColor, ratio) * _emissionIntensity;
        _materialInstance.SetColor(EmissionColorId, emissionColor);
    }
}