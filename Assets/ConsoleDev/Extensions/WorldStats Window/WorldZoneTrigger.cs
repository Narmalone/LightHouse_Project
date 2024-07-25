using System;
using UnityEngine;

public class WorldZoneTrigger : MonoBehaviour
{
    public static event Action<WorldStatsWindow.Zone> OnZoneChange;

    [SerializeField] private LayerMask layer;
    [SerializeField] private WorldStatsWindow.Zone ZoneTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if ((layer & (1 << other.gameObject.layer)) > 0)
        {
            OnZoneChange?.Invoke(ZoneTrigger);
        }
    }
}
