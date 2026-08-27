using LightHouse.Features.Interactions;
using System;
using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    #region EVENTS

    public event Action OnObjectInteracted;
    public event Action OnInteractionNameChanged;
    public event Action<string> OnNameUpdated;

    #endregion


    #region INTERACTION

    [field: SerializeField]
    public string InteractionText { get; set; } = "Allumer la lumière";

    [field: SerializeField]
    public bool CanBeInteracted { get; set; } = true;

    [field: SerializeField]
    public bool CanBeRaycasted { get; set; } = true;

    public bool IsItemRaycasted { get; set; }

    #endregion


    #region REFERENCES

    [Header("Light")]
    [SerializeField] private CeilingLight _ceilingLight;

    [Header("Interaction")]
    [SerializeField] private Collider _interactionCollider;

    [Header("Visual")]
    [SerializeField] private Transform _switchVisual;

    [SerializeField] private Vector3 _offRotation;
    [SerializeField] private Vector3 _onRotation;

    #endregion


    #region STATE

    private bool _isOn;

    #endregion


    #region UNITY

    private void Awake()
    {
        if (_interactionCollider == null)
            _interactionCollider = GetComponent<Collider>();

        if (_interactionCollider == null)
            Debug.LogError(
                $"{name} possède un LightSwitch mais aucun Collider !",
                this
            );

        if (_ceilingLight == null)
            Debug.LogError(
                $"{name} : CeilingLight n'est pas assignée !",
                this
            );

        UpdateInteractionText();
        UpdateVisual();
    }

    #endregion


    #region INTERACTION

    public void Interact()
    {
        Debug.Log($"Interaction avec {name}", this);

        if (!CanBeInteracted)
        {
            Debug.LogWarning($"{name} ne peut pas être utilisé.", this);
            return;
        }

        _isOn = !_isOn;

        if (_ceilingLight != null)
        {
            if (_isOn)
                _ceilingLight.UserTurnOn();
            else
                _ceilingLight.UserTurnOff();
        }

        UpdateVisual();
        UpdateInteractionText();

        OnObjectInteracted?.Invoke();
    }

    #endregion


    #region VISUAL

    private void UpdateVisual()
    {
        if (_switchVisual == null)
            return;

        _switchVisual.localEulerAngles =
            _isOn
                ? _onRotation
                : _offRotation;
    }

    #endregion


    #region UI

    private void UpdateInteractionText()
    {
        InteractionText =
            _isOn
                ? "Éteindre la lumière"
                : "Allumer la lumière";

        OnInteractionNameChanged?.Invoke();
        OnNameUpdated?.Invoke(InteractionText);
    }

    #endregion


    #region IITEMNAME

    public string GetName()
    {
        return InteractionText;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Collider GetCollider()
    {
        return _interactionCollider;
    }

    #endregion
}