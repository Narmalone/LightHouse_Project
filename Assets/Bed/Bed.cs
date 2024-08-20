using System;
using UnityEngine;

public class Bed : ItemBase
{
    [Header("Events")]
    [SerializeField] private CustomEvent _eventEvening;
    [SerializeField] private CustomEvent _eventMorning;
    [SerializeField] private CustomEvent _eventFadeIsMasking;
    [SerializeField] private CustomEvent_Float _eventSetTime;
    [SerializeField] private CustomEvent_Float _eventFade;
    [SerializeField] private CustomEvent_String _eventCrossaireNameUpdate;

    [Header("Stats")]
    [SerializeField] private bool _forEnableSleep;
    [SerializeField] private float _sleepingTime;

    private LayerMask _defaultMask;

    private void Awake()
    {
        Name = "Sleep";

        _eventEvening.handle += OnEvening;
        _eventMorning.handle += OnMorning;
    }

    private void Start()
    {
        isUsable = false;
        _defaultMask.value = gameObject.layer;
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnEvening;
        _eventMorning.handle -= OnMorning;
    }

    public override void Use()
    {
        base.Use();

        // Lock Mouse input

        // MouvemntCamera

        // Sleep
        _eventFade.Raise(_sleepingTime);
        _eventFadeIsMasking.handle += SetTimeMorning;

        // Unlock Mouse input

    }

    private void SetTimeMorning()
    {
        _eventSetTime.Raise(6.5f);

        _eventFadeIsMasking.handle -= SetTimeMorning;
    }

    private void OnMorning()
    {
        isUsable = _forEnableSleep;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void OnEvening()
    {
        isUsable = true;
        gameObject.layer = LayerMask.NameToLayer("Items");
    }

}
