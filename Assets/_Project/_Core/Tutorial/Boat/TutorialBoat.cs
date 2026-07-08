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
            PlayerHandlerData.MainPlayer.Character.SetPosition(_playerSpawnTutorial.position);
            PlayerHandlerData.MainPlayer.Character.SetRotation(_playerSpawnTutorial.rotation);
        }

    }
}
