using LightHouse.Features.Electricity;
using System;
using UnityEngine;
public class ElectricalSlot : MonoBehaviour
{
    [SerializeField] private ElectricityZones _zone;
    [SerializeField] private Transform _fuzePanelPivot;
    [SerializeField] private ElectricDurabilityController _durabilityController;
    [SerializeField] private FuzeButtonController _fuzeButtonController;
    [SerializeField] private FuzeSwitchController _fuzeSwitchController;
    [SerializeField] private FuzeItemNeeded _fuzeItemNeeded;
    [SerializeField] private IElectricItem[] _electricalItems;

    public ElectricityZones Zone => _zone;

    /// <summary>
    /// Raised whenever this slot's switch turns the zone's electricity on/off,
    /// so ElectricTab can aggregate it into a single tab-wide event
    /// (mirrors what the old ElectricalPannel used to broadcast per zone).
    /// </summary>
    public event Action<ElectricityZones, bool> OnElectricityStateChanged;

    private void Awake()
    {
        _durabilityController.OnDurabilityEnded += HandleDurabilityEnded;
        _fuzeItemNeeded.OnFuzeSet += HandleFuzeSet;
        _fuzeItemNeeded.OnFuzeRemoved += HandleFuzeRemoved;
        _fuzeSwitchController.OnSwitchPressedEvent += FuzeSwitchController_OnSwitchPressedEvent;
        _fuzeSwitchController.OnObjectInteracted += FuzeSwitchController_OnObjectInteracted;
    }

    private void OnDestroy()
    {
        _durabilityController.OnDurabilityEnded -= HandleDurabilityEnded;
        _fuzeItemNeeded.OnFuzeSet -= HandleFuzeSet;
        _fuzeItemNeeded.OnFuzeRemoved -= HandleFuzeRemoved;
        _fuzeSwitchController.OnSwitchPressedEvent -= FuzeSwitchController_OnSwitchPressedEvent;
        _fuzeSwitchController.OnObjectInteracted -= FuzeSwitchController_OnObjectInteracted;
    }

    private void FuzeSwitchController_OnObjectInteracted()
    {
        if (_fuzeItemNeeded.HasFuze)
        {
            _fuzeSwitchController.OnSwitchPressed();
        }
    }

    private void FuzeSwitchController_OnSwitchPressedEvent()
    {
        _durabilityController.SetActiveDurability(_fuzeSwitchController.IsOn);
        OnElectricityStateChanged?.Invoke(_zone, _fuzeSwitchController.IsOn);
    }

    private void HandleFuzeSet(float durability, float maxDurability)
    {
        _durabilityController.SetDurability(durability, maxDurability);
        _fuzeSwitchController.OnFuzeSet();
    }

    private void HandleFuzeRemoved()
    {
        _fuzeSwitchController.SetOff();
        _durabilityController.SetActiveDurability(false);
        _fuzeSwitchController.OnFuzeRemoved();
        OnElectricityStateChanged?.Invoke(_zone, false);
    }

    private void HandleDurabilityEnded()
    {
        // Handle the event when durability ends
        Debug.Log("Durability ended!");
        Shutdown();
    }

    public void SetFuzeItemToSlot(FuzeItem fuzeTarget)
    {
        fuzeTarget.GetRigidBody().isKinematic = true;
        fuzeTarget.transform.SetParent(_fuzePanelPivot);
    }

    /// <summary>
    /// Enables/disables player interaction on this slot's switch.
    /// Mirrors the old panel's OnEnable/DisablePannelInteractibility, now per-slot.
    /// Adjust if FuzeSwitchController exposes a more specific interactibility toggle.
    /// </summary>
    public void SetInteractable(bool value)
    {
        _fuzeSwitchController.enabled = value;
    }

    public void Shutdown(bool destroyFuze = true)
    {
        // Implement shutdown logic here
        Debug.Log("Electrical slot is shutting down.");
        _fuzeSwitchController.SetOff();
        _durabilityController.SetActiveDurability(false);
        OnElectricityStateChanged?.Invoke(_zone, false);
    }
}