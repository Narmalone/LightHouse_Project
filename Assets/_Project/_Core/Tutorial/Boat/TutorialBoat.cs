using LightHouse.Core.Player;
using LightHouse.Features.Boats;
using UnityEngine;

namespace LightHouse.Core.Tutorial.Boat
{
    public class TutorialBoat : MonoBehaviour
    {
        [SerializeField] private BoatPathMover _boid;
        [SerializeField] private VectorPath _targetPath;
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private Collider[] _subBoatColliders;

        public Collider[] SubBoatColliders => _subBoatColliders;
        public BoatPathMover BoatPathMover => _boid;

        public Transform _playerSpawnTutorial;

        public void InitializeBoat()
        {
            _boid.InitializeOnPath();
        }

        public void SpawnPlayerOnBoatPos()
        {
            if (PlayerHandlerData.MainPlayer != null)
            {
                PlayerHandlerData.MainPlayer.Inventory.Disable();
                PlayerHandlerData.MainPlayer.Interactions.Disable();
                PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = false;
                PlayerHandlerData.MainPlayer.EnableCameraRotationInput = false;
            }

            PlayerHandlerData.MainPlayer.Character.ForceCutVelocity();
            PlayerHandlerData.MainPlayer.Character.ForceLookRotation(_playerSpawnPoint.rotation);

            PlayerHandlerData.MainPlayer.Character.SetPositionAndRotation(
            _playerSpawnTutorial.position,
            _playerSpawnTutorial.rotation,
            true);

            PlayerHandlerData.MainPlayer.PlayerCamera.SetRotation(
                _playerSpawnTutorial.rotation);

        }

    }
}
