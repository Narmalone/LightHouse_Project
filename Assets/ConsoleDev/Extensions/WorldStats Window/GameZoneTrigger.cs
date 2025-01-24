using System;
using UnityEngine;

public class GameZoneTrigger : MonoBehaviour
{
    [SerializeField] private CustomEvent_GameZone _onZoneChangedRaise;
    [SerializeField] private CustomEvent_GameZone _onZoneChangedListener;
    [SerializeField] private LayerMask layer;
    [SerializeField] private GameZone ZoneTrigger;

    [SerializeField] private Collider _collider;

    private void Awake()
    {
        _onZoneChangedListener.handle += _onZoneChangedListener_handle;
    }

    private void OnDestroy()
    {
        _onZoneChangedListener.handle -= _onZoneChangedListener_handle;
    }

    private void _onZoneChangedListener_handle(GameZone obj)
    {
        if (obj == this.ZoneTrigger) return;

        if (!_collider.enabled) _collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((layer & (1 << other.gameObject.layer)) > 0)
        {
            if (GameManager.CurrentPlayerZone == ZoneTrigger) return;
            GameManager.CurrentPlayerZone = ZoneTrigger;
            _onZoneChangedRaise?.Raise(ZoneTrigger);
            if (_collider.enabled) _collider.enabled = false;
        }
    }
}
