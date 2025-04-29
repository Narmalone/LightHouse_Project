using UnityEngine;

namespace LightHouse.KinematicCharacterController
{
    public class CameraLean : MonoBehaviour
    {
        [SerializeField] private float _attackDamping = 0.5f; //input acceleration
        [SerializeField] private float _decayDamping = 0.3f;
        [SerializeField] private float _strength = 0.075f;
        private Vector3 _dampedAcceleration;
        private Vector3 _dampedAccelerationVel;

        public void Initialize()
        {

        }

        public void UpdateLean(float deltaTime, Vector3 acceleration, Vector3 up)
        {
            var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
            var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
                ? _attackDamping
                : _decayDamping;

            _dampedAcceleration = Vector3.SmoothDamp
                (
                    current: _dampedAcceleration,
                    target: planarAcceleration,
                    currentVelocity: ref _dampedAccelerationVel,
                    smoothTime: damping,
                    maxSpeed: float.PositiveInfinity,
                    deltaTime: deltaTime
                );
            var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
            transform.localRotation = Quaternion.identity;
            transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _strength, leanAxis) * transform.rotation;
        }
    }
}
