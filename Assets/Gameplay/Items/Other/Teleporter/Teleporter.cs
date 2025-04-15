using System;
using UnityEngine;

namespace LightHouse.KinematicCharacterController
{
    public class Teleporter : MonoBehaviour
    {
        [SerializeField] private Teleporter _teleportTo;

        public event Action<PlayerCharacter> OnPlayerTeleported;
        public bool isBeingTeleportedTo { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBeingTeleportedTo)
            {
                PlayerCharacter cc = other.GetComponent<PlayerCharacter>();
                if (cc)
                {
                    cc.Motor.SetPositionAndRotation(_teleportTo.transform.position, _teleportTo.transform.rotation);
                    OnPlayerTeleported?.Invoke(cc);
                    _teleportTo.isBeingTeleportedTo = true;
                }
            }
            isBeingTeleportedTo = false;
        }
    }

}
