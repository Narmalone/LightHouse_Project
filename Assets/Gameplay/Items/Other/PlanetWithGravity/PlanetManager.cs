using KinematicCharacterController;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.KinematicCharacterController
{
    public class PlanetManager : MonoBehaviour, IMoverController
    {
        public PhysicsMover PlanetMover;
        public SphereCollider GravityField;
        public float GravityStrength = 10;
        public Vector3 OrbitAxis = Vector3.forward;
        public float OrbitSpeed = 10;

        public LightHouse.KinematicCharacterController.Teleporter OnPlaygroundTeleportingZone;
        public LightHouse.KinematicCharacterController.Teleporter OnPlanetTeleportingZone;

        private List<PlayerCharacter> _characterControllersOnPlanet = new List<PlayerCharacter>();
        private Vector3 _savedGravity;
        private Quaternion _lastRotation;

        private void Start()
        {
            OnPlaygroundTeleportingZone.OnPlayerTeleported -= ControlGravity;
            OnPlaygroundTeleportingZone.OnPlayerTeleported += ControlGravity;

            OnPlanetTeleportingZone.OnPlayerTeleported -= UnControlGravity;
            OnPlanetTeleportingZone.OnPlayerTeleported += UnControlGravity;

            _lastRotation = PlanetMover.transform.rotation;

            PlanetMover.MoverController = this;
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = PlanetMover.Rigidbody.position;

            // Rotate
            Quaternion targetRotation = Quaternion.Euler(OrbitAxis * OrbitSpeed * deltaTime) * _lastRotation;
            goalRotation = targetRotation;
            _lastRotation = targetRotation;

            // Apply gravity to characters
            foreach (PlayerCharacter cc in _characterControllersOnPlanet)
            {
                cc.Gravity = (PlanetMover.transform.position - cc.transform.position).normalized * GravityStrength;
            }
        }

        void ControlGravity(PlayerCharacter cc)
        {
            _savedGravity = cc.Gravity;
            _characterControllersOnPlanet.Add(cc);
        }

        void UnControlGravity(PlayerCharacter cc)
        {
            cc.Gravity = _savedGravity;
            _characterControllersOnPlanet.Remove(cc);
        }
    }

}
