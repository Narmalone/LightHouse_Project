using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReloadingLightHouseWeight : ItemBase
{
    [Header("Event")]
    [SerializeField] private CustomEvent _eventFreeze;
    [SerializeField] private CustomEvent _eventUnfreeze;
    [SerializeField] private CustomEvent _eventRotationOn;
    [SerializeField] private CustomEvent _eventOnReloadingWeight;
    [SerializeField] private CustomEvent _eventEndReloadingWeight_Cursor;
    [SerializeField] private CustomEvent_2Float _eventOnReloadingWeight_Cursor;
    [SerializeField] private CustomEvent _eventMorning;
    [SerializeField] private CustomEvent _eventEvening;

    [Header("Component")]
    [SerializeField] private Animator _animator;

    [Header("Stats")]
    [SerializeField] private float _timeHoldToReload;

    private int _hasWeight = Animator.StringToHash("Weight");

    private Coroutine _coroutineOnReloading;
    private float _startTimeHold;
    private float _tempStartTime;

    private PIA _input;

    private void Awake()
    {
        _input = new PIA();

        _input.Enable();

        _eventEvening.handle += OnAllowRotationOn;
        _eventMorning.handle += OnStopRotation;
    }
    private void Start()
    {
        Name = name;
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnAllowRotationOn;
        _eventMorning.handle -= OnStopRotation;
    }

    public override bool Use()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");

        _input.Game.Interact.canceled += OnInteractCancel;

        _startTimeHold = Time.time - _tempStartTime;

        _coroutineOnReloading = StartCoroutine(OnReloading());

        _eventOnReloadingWeight_Cursor.Raise(_timeHoldToReload, _tempStartTime / _timeHoldToReload);
        _eventFreeze.Raise();

        _animator.SetFloat(_hasWeight, 1);

        return base.Use();
    }

    private void OnInteractCancel(InputAction.CallbackContext obj)
    {
        if (_startTimeHold == -1) return;
        StopCoroutine(_coroutineOnReloading);            
        Stop(Time.time - _startTimeHold >= _timeHoldToReload);

        _input.Game.Interact.canceled -= OnInteractCancel;
    }

    private void OnStopRotation()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void OnAllowRotationOn()
    {
        gameObject.layer = LayerMask.NameToLayer("Items");
    }

    private void Stop(bool finished)
    {
        _animator.SetFloat(_hasWeight, 0);
        _eventUnfreeze.Raise();
        _eventEndReloadingWeight_Cursor.Raise();
        if (finished == false) 
        {
            _tempStartTime = Time.time - _startTimeHold;
            gameObject.layer = LayerMask.NameToLayer("Items");
            return;
        }
        _tempStartTime = 0;
        _startTimeHold = -1;
        _eventRotationOn.Raise();
    }

    IEnumerator OnReloading()
    {
        while (Time.time - _startTimeHold < _timeHoldToReload)
        {
            //Debug.Log(Time.time - _startTimeHold);
            _eventOnReloadingWeight.Raise();
            yield return null;
        }

        Stop(true);
        _coroutineOnReloading = null;
    }
}