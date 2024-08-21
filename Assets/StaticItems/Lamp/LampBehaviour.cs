using UnityEngine;

public class LampBehaviour : ElectricItem
{
    [SerializeField] private Light _light;
    public bool IsItemOn => _light.enabled;

    public override void OnElecEnabled()
    {
        _light.enabled = true;
    }

    public override void OnElecDisabled()
    {
        _light.enabled = false;
    }

    private void Awake()
    {
        if(_light.enabled) _light.enabled = false;
    }
}
