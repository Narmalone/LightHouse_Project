using System;
using UnityEngine;

public class Bed : ItemBase
{
    [SerializeField] private CustomEvent _eventEvening;
    [SerializeField] private CustomEvent _eventMorning;
    [SerializeField] private CustomEvent_String _eventCrossaireNameUpdate;
    [SerializeField] private bool _forEnableSleep;

    private void Awake()
    {
        _eventEvening.handle += OnEvening;
        _eventMorning.handle += OnMorning;
    }

    private void Start()
    {
        isUsable = false;
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnEvening;
        _eventMorning.handle -= OnMorning;
    }

    public override void Use()
    {
        base.Use();
    }

    private void OnMorning()
    {
        isUsable = _forEnableSleep;
    }

    private void OnEvening()
    {
        isUsable = true;
    }

}
