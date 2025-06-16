using LightHouse.Game.Computer.NightWatch.Sonar;
using UnityEngine;
using LightHouse.Game.WaterExtension;

namespace LightHouse.Game.Boats
{
    public class Boat : MonoBehaviour, ISonarable
    {
        public PhysicalBuoyancy[] Buoyancies;

        private string _currentName;
        public string Name => _currentName;

        [field: SerializeField] public int UniqueID { get; set; } = -1;
        [field: SerializeField] public bool IsDetectedBySonar { get; set; }
        public Vector3 Position => this.transform.position;
        public Vector3 RotationAngles => this.transform.eulerAngles;
        [field: SerializeField] public Color DotColor { get; set; }
        [field: SerializeField] public Vector2 DotSize { get; set; }

        [SerializeField] private RandomPointOnWaterSurface _randomPointOnWaterSurface;
        [SerializeField] private float _movementSpeed = 5.0f;
        [SerializeField] private float _orientationSpeed = 2.0f; // <--- en radians/seconde
        [SerializeField] private Rigidbody _rigidbody;

        private Vector3 _targetPosition;

        private void Start()
        {
            _targetPosition = _randomPointOnWaterSurface.Destination;
        }

        private void FixedUpdate()
        {
            SteerTowardsTarget();
            MoveForward();
        }

        private void SteerTowardsTarget()
        {
            Vector3 directionToTarget = (_targetPosition - _rigidbody.position).normalized;

            // Ignore la composante verticale
            directionToTarget.y = 0f;
            if (directionToTarget == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            _rigidbody.rotation = Quaternion.RotateTowards(
                _rigidbody.rotation,
                targetRotation,
                _orientationSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime // passage en degrés/s
            );
        }

        private void MoveForward()
        {
            Vector3 forward = _rigidbody.transform.forward;
            _rigidbody.AddForce(forward * _movementSpeed, ForceMode.Force);
        }
    }
}

