using Cinemachine;
using System;
using UnityEngine;

public class Bed : ItemBase
{
    [Header("Events")]
    [SerializeField] private CustomEvent _eventEvening;
    [SerializeField] private CustomEvent _eventMorning;
    [SerializeField] private CustomEvent _eventFadeIsMasking;
    [SerializeField] private CustomEvent _eventFadeEnd;
    [SerializeField] private CustomEvent _eventLockCameraPlayer;
    [SerializeField] private CustomEvent _eventUnlockCameraPlayer;
    [SerializeField] private CustomEvent _eventLockMovementPlayer;
    [SerializeField] private CustomEvent _eventUnlockMovementPlayer;
    [SerializeField] private CustomEvent _eventResetSleepAmount;
    [SerializeField] private CustomEvent_Float _eventSetTime;
    [SerializeField] private CustomEvent_Float _eventFade;
    [SerializeField] private CustomEvent_String _eventCrossaireNameUpdate;

    [Header("Components")]
    [SerializeField] private CinemachineVirtualCamera _VCam;

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
        ForceUnusable();

        // Lock Mouse input
        _eventLockCameraPlayer.Raise();
        _eventLockMovementPlayer.Raise();

        // MouvemntCamera
        _VCam.Priority = 100;

        // Sleep
        _eventFade.Raise(_sleepingTime);
        _eventFadeIsMasking.handle += SetTimeMorning;
        _eventFadeEnd.handle += UnlockPlayer;

    }

    private void SetTimeMorning()
    {
        _eventResetSleepAmount.Raise();
        _eventSetTime.Raise(6.5f);
        isUsable = _forEnableSleep;

        _VCam.Priority = 0;

        _eventFadeIsMasking.handle -= SetTimeMorning;
    }

    private void UnlockPlayer()
    {
        // Unlock Mouse input
        _eventUnlockCameraPlayer.Raise();
        _eventUnlockMovementPlayer.Raise();

        _eventFadeEnd.handle -= UnlockPlayer;
    }

    private void ForceUnusable()
    {
        isUsable = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
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
