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

    [SerializeField] private bool _isActive = false;

    private int _hashSwitch = Animator.StringToHash("Switch");

    private void Awake()
    {
        _eventEvening.handle += OnAllowLightOn;
        _eventMorning.handle += OnStopLight;
    }

    private void Start()
    {
        Name = name;
        ActiveDesactiveLigh(false);
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnAllowLightOn;
        _eventMorning.handle -= OnStopLight;
    }

    public override bool Use()
    {
        _isActive = !_isActive;
        ActiveDesactiveLigh(_isActive);
        return _isActive;
    }

    private void OnStopLight()
    {
        ActiveDesactiveLigh(false);
        EnableRaycastDetection = false;
    }

    private void ActiveDesactiveLigh(bool active)
    {
        if (active) _eventLightOn.Raise();
        else _eventLightOff.Raise();


        //_animatorSwitch.SetBool(_hashSwitch, active);


    }
    private void OnAllowLightOn()
    {
        EnableRaycastDetection = true;
    }
}