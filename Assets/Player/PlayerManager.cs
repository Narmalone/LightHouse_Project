using System;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] public PlayerController _controller;
    [SerializeField] public PlayerInventory _inventory;
    [SerializeField] public PlayerInteraction _interaction;
    [SerializeField] public PlayerData _data;

    public Action _eventUpdate;

    public bool Freeze;

    protected override void Awake()
    {
        _data._eventFreeze.handle += OnFreeze;
        _data._eventUnfreeze.handle += OnUnfreeze;

        _inventory?.Initialize(this);
        _controller?.Initialize(this);
        _interaction?.Initialize(this);
        _data?.Initialize(this);
    }

    private void Update()
    {
        if (Freeze) return;
        _eventUpdate.Invoke();
    }

    private void OnDestroy()
    {
        _data._eventFreeze.handle -= OnFreeze;
        _data._eventUnfreeze.handle -= OnUnfreeze;
    }
    internal void SetSpeedWhenIsGrabbing(float ratio)
    {
        _controller._rationSpeedWhenGrabbing = ratio;
    }
    private void OnFreeze()
    {
        Freeze = true;
    }

    private void OnUnfreeze()
    {
        Freeze = false;
    }
}