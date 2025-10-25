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

        [Header("Sensitivity")]
        public float SensiX = 1f;
        public float SensiY = 1f;
        public float FOV = 80f;
        public float DefaultFOV = 90f;

        public Transform Transform { get; private set; }
        [field: SerializeField] public Transform FollowTransform { get; private set; }

        public Vector3 PlanarDirection { get; set; }

        [SerializeField] private CinemachineVirtualCamera _cm;
        public CinemachineVirtualCamera CinemachineCamera => _cm;

        [SerializeField] private Camera _playerCamera;
        public Camera Camera => _playerCamera;

        private float _targetVerticalAngle;

        void OnValidate()
        {
            DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
            if (_cm != null)
                _cm.m_Lens.FieldOfView = FOV;
        }

        void Awake()
        {
            Transform = this.transform;
            _targetVerticalAngle = 0f;
            PlanarDirection = Vector3.forward;
        }

        private void Start()
        {
            PlanarDirection = FollowTransform.forward;
            Transform.position = FollowTransform.position;
        }

        public void SetFov(float fov)
        {
            if (_cm != null)
                _cm.SetFieldOfView(fov);
            if (_playerCamera != null)
                _playerCamera.fieldOfView = fov;
        }

        public void UpdateWithInput(float deltaTime, Vector3 rotationInput)
        {
            if (!FollowTransform)
                return;

            // appliquer sensi
            rotationInput.x *= SensiX;
            rotationInput.y *= SensiY;

            if (InvertX) rotationInput.x *= -1f;
            if (InvertY) rotationInput.y *= -1f;

            // --- ROTATION INSTANTANÉE ---

            // yaw (horizontal) => fait tourner la direction plane
            Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * (rotationInput.x * RotationSpeed));
            PlanarDirection = rotationFromInput * PlanarDirection;

            // recoller PlanarDirection au plan horizontal du FollowTransform
            PlanarDirection = Vector3.Cross(
                FollowTransform.up,
                Vector3.Cross(PlanarDirection, FollowTransform.up)
            );

            Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);

            // pitch (vertical)
            _targetVerticalAngle -= (rotationInput.y * RotationSpeed);
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);

            Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0f, 0f);

            // rotation finale sans Slerp / sans lissage
            Quaternion finalRot = planarRot * verticalRot;
            Transform.rotation = finalRot;

            // --- POSITION INSTANTANÉE ---
            //Transform.position = FollowTransform.position;
        }
    }
}
