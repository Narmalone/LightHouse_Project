using Cinemachine;
using UnityEngine;

namespace LightHouse.KinematicCharacterController
{
    [System.Serializable]
    public struct CameraInput
    {
        public Vector2 Look;
    }

    public class PlayerCamera : MonoBehaviour
    {
        [Header("Rotation")]
        public bool InvertX = false;
        public bool InvertY = false;

        [Range(-90f, 90f)] public float DefaultVerticalAngle = 20f;
        [Range(-90f, 90f)] public float MinVerticalAngle = -90f;
        [Range(-90f, 90f)] public float MaxVerticalAngle = 90f;

        public float RotationSpeed = 1f;
        public float RotationSharpness = 10000f;

        public float PositionSharpness = 10000f;
        public float DampSmoothTime = 0.05f;

        [Header("Sensitivity")]
        public float SensiX = 1f;
        public float SensiY = 1f;
        public float FOV = 80f;
        public float DefaultFOV = 90f;

        public Transform Transform { get; private set; }
        public Transform FollowTransform { get; private set; }

        public Vector3 PlanarDirection { get; set; }

        [SerializeField] private CinemachineVirtualCamera _cm;
        public CinemachineVirtualCamera CinemachineCamera => _cm;

        [SerializeField] private Camera _playerCamera;
        public Camera Camera => _playerCamera;

        private float _targetVerticalAngle;
        private Vector3 _currentFollowPosition;
        private Vector3 _velocitySmoothing;

        void OnValidate()
        {
            DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
            _cm.m_Lens.FieldOfView = FOV;
        }

        void Awake()
        {
            Transform = this.transform;
            _targetVerticalAngle = 0f;
            PlanarDirection = Vector3.forward;
        }

        public void SetFollowTransform(Transform t)
        {
            FollowTransform = t;
            PlanarDirection = FollowTransform.forward;
            _currentFollowPosition = FollowTransform.position;
        }

        public void SetFov(float fov)
        {
            _cm.SetFieldOfView(fov);
        }

        public void UpdateWithInput(float deltaTime, Vector3 rotationInput)
        {
            if (FollowTransform)
            {
                rotationInput.x *= SensiX;
                rotationInput.y *= SensiY;

                if (InvertX)
                {
                    rotationInput.x *= -1f;
                }
                if (InvertY)
                {
                    rotationInput.y *= -1f;
                }

                // Rotation input → planar direction
                Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * (rotationInput.x * RotationSpeed));
                PlanarDirection = rotationFromInput * PlanarDirection;
                PlanarDirection = Vector3.Cross(FollowTransform.up, Vector3.Cross(PlanarDirection, FollowTransform.up));
                Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);

                _targetVerticalAngle -= (rotationInput.y * RotationSpeed);
                _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
                Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0, 0);
                Quaternion targetRotation = Quaternion.Slerp(Transform.rotation, planarRot * verticalRot, 1f - Mathf.Exp(-RotationSharpness * deltaTime));

                // Apply rotation
                Transform.rotation = targetRotation;

                // Position follow
                //_currentFollowPosition = Vector3.Lerp(_currentFollowPosition, FollowTransform.position, 1f - Mathf.Exp(-PositionSharpness * deltaTime));
                _currentFollowPosition = Vector3.SmoothDamp(
                    _currentFollowPosition,
                    FollowTransform.position,
                    ref _velocitySmoothing,
                    DampSmoothTime // délai d'amortissement
                );

                Vector3 targetPosition = _currentFollowPosition;

                // Apply position
                Transform.position = targetPosition;
            }
        }
    }
}
