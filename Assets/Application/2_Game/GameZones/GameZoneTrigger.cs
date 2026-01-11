using LightHouse.Core.Utilities;
using UnityEngine;

namespace LightHouse.Core.World
{
    public class GameZoneTrigger : TriggerEvent
    {
        [SerializeField] private ZoneType Zone;

        protected override void OnTriggerEntered()
        {
            GameZoneHandlerData.SetGameZone(this.Zone);
        }
    }

}
