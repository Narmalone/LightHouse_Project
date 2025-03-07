using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] public PlayerController _controller;
    [SerializeField] public PlayerData _data;

    public Action _eventUpdate;

    public bool Freeze;

    protected void Awake()
    {
        _data._eventFreeze += OnFreeze;
        _data._eventUnfreeze += OnUnfreeze;

        _controller?.Initialize(this);
        _data?.Initialize(this);
    }

    private void Update()
    {
        if (Freeze) return;
        _eventUpdate.Invoke();
    }

    private void OnDestroy()
    {
        _data._eventFreeze -= OnFreeze;
        _data._eventUnfreeze -= OnUnfreeze;
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