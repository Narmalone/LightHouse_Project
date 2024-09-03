using UnityEngine;

public class SwitchLightHouse: ItemBase
{
    [Header("Event")]
    [SerializeField] CustomEvent _eventLightOn;
    [SerializeField] CustomEvent _eventLightOff;
    [SerializeField] CustomEvent _eventMorning;
    [SerializeField] CustomEvent _eventEvening;

    [Header("Components")]
    [SerializeField] Animator _animatorSwitch;

    private int _hashSwitch = Animator.StringToHash("Switch");

    private void Awake()
    {
        _eventEvening.handle += OnAllowLightOn;
        _eventMorning.handle += OnStopLight;
    }

    private void Start()
    {
        Name = name;
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnAllowLightOn;
        _eventMorning.handle -= OnStopLight;
    }

    public override bool Use()
    {
        ActiveDesactiveLigh(true);
        gameObject.layer = LayerMask.NameToLayer("Default");

        return base.Use();
    }

    private void OnStopLight()
    {
        ActiveDesactiveLigh(false);
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void ActiveDesactiveLigh(bool active)
    {
        if(active) _eventLightOn.Raise();
        else _eventLightOff.Raise();

        _animatorSwitch.SetBool(_hashSwitch, active);
    }

    private void OnAllowLightOn()
    {
        gameObject.layer = LayerMask.NameToLayer("Items");
    }
}