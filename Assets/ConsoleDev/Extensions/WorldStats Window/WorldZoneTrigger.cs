using System;
using UnityEngine;

public class WorldZoneTrigger : MonoBehaviour
{
    public static event Action<GameZone> OnZoneChange;

    [SerializeField] private LayerMask layer;
    [SerializeField] private GameZone ZoneTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if ((layer & (1 << other.gameObject.layer)) > 0)
        {
            OnZoneChange?.Invoke(ZoneTrigger);
        }
    }
}
