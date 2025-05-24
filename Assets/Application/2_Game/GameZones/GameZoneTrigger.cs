using LightHouse.Utilities;
using UnityEngine;

namespace LightHouse.Game.World
{
    public class GameZoneTrigger : TriggerEvent
    {
        [SerializeField] private GameZone Zone;

        protected override void OnTriggerEntered()
        {
            GameZoneHandlerData.SetGameZone(this.Zone);
        }
    }

}
