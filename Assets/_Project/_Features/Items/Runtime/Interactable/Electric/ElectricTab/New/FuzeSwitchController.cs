using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;
using LightHouse.Features.Items.Interactable;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FuzeSwitchController : InteractableItemBase
{
    public event Action OnSwitchPressedEvent;

    [SerializeField] private Transform _fuzeOnTransform;
    [SerializeField] private Transform _fuzeOffTransform;
    [SerializeField] private Vector3 _eulerTargetOn;
    [SerializeField] private Vector3 _eulerTargetOff;

    private bool _isOn = false;
    private bool _isFuzeBroke = false;

    public bool IsOn => _isOn;

    protected override void Start()
    {
        base.Start();
        SetOff();
    }

    public void OnFuzeShutdown()
    {
        _isOn = false;
    }

    public void OnFuzeBroke()
    {
        _isFuzeBroke = true;
        _isOn = false;
    }

    public async Task<string> GetOnInteractionText()
    {
        string input = InputManager.Interact_Bind_Name;
        if (_isOn)
        {
            //Take the on text localized string
        }
        else
        {
            //Take off text localized string
        }

        var interactionName = await InteractionTextBuilder.Build_Hold_To_Action(
                _interactText,
                input,
                _pressToAction
            );
        return interactionName;
    }

    public void OnSwitchPressed()
    {
        if (_isFuzeBroke) return;
        _isOn = !_isOn;

        if(_isOn)
        {
            SetOn();
        }
        else
        {
            SetOff();
        }

        OnSwitchPressedEvent?.Invoke();
    }

    public void SetOn()
    {
        _isOn = true;
        _fuzeOnTransform.localEulerAngles = _eulerTargetOn;
        _fuzeOffTransform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public void SetOff()
    {
        _isOn = false;
        _fuzeOffTransform.localEulerAngles = _eulerTargetOff;
        _fuzeOnTransform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public override void Interact()
    {
        InvokeObjectInteracted();
    }
}
