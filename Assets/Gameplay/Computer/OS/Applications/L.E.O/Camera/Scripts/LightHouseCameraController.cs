using Cinemachine;
using LightHouse.Game.DayNightSystem;
using LightHouse.Inputs;
using System;
using UnityEngine;

namespace LightHouse.Game.Computer.Cameras
{
    public class LightHouseCameraController : MonoBehaviour
    {
        [Header("Camera")]
        public CinemachineVirtualCamera LightHouseCamera;
        [SerializeField] private CameraToUI _renderTextureCamera;
        public bool IsEnabled = false;
        public float sensitivity = 1.5f;
        public float verticalClamp = 80f;

        [Header("Input Axis")]
        public bool invertX = false;
        public bool invertY = true;

        [Header("Zoom Settings")]
        public float minZoomFOV = 25f;
        public float maxZoomFOV = 60f;
        [Range(0f, 1f)] public float zoomValue = 0.5f; // Contrôlé par un slider
        public float zoomSmoothSpeed = 10f;

        private float yaw;
        private float pitch;
        private Transform cameraTarget;
        private float currentFOV;
        public CameraToUI RenderTextureCamera => _renderTextureCamera;
        public event Action<float> OnZoomChanged;

        private Quaternion _initialRotation;
        [SerializeField, Range(0, 20)] private int resetEachFrames = 5;

        private void Start()
        {
            if (LightHouseCamera != null)
            {
                cameraTarget = LightHouseCamera.transform;
                currentFOV = LightHouseCamera.m_Lens.FieldOfView;

                _initialRotation = cameraTarget.rotation;

                // Décomposer la rotation initiale en yaw/pitch pour conserver la bonne orientation
                Vector3 euler = _initialRotation.eulerAngles;
                yaw = euler.y;
                pitch = euler.x;
            }
        }


        public void SetEnable(bool value)
        {
            IsEnabled = value;
            _renderTextureCamera.RenderOnce();
        }

        public void SetEnableFullScren(bool value)
        {
            LightHouseCamera.Priority = value ? 100 : -1;
        }

        private void Update()
        {
            if (!IsEnabled) return;

            Vector2 moveInput = InputManager.PLAYER_INPUTS_ACTIONS.Computer.CameraControl_Move.ReadValue<Vector2>();
            float scrollInput = InputManager.PLAYER_INPUTS_ACTIONS.Computer.CameraControl_Zoom.ReadValue<Vector2>().y;

            UpdateCameraRotationFromKeys(moveInput);

            // Scroll input affecte zoomValue (chaque cran = 0.1f)
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                SetZoomValue(zoomValue + scrollInput * 0.1f);
            }

            UpdateZoom();

            LightHouseCamera.m_Lens.FieldOfView = currentFOV;
            _renderTextureCamera.renderCam.fieldOfView = currentFOV;
        }


        private void LateUpdate()
        {
            if (Time.frameCount % resetEachFrames == 0 && IsEnabled)
            {
                _renderTextureCamera.RenderOnce();
            }
        }

        private void UpdateCameraRotationFromKeys(Vector2 moveInputs)
        {
            if (moveInputs == Vector2.zero || cameraTarget == null) return;

            // moveInputs.x = -1 (Q), +1 (D)
            // moveInputs.y = +1 (Z), -1 (S)

            float deltaX = moveInputs.x * sensitivity * (invertX ? -1 : 1);
            float deltaY = moveInputs.y * sensitivity * (invertY ? -1 : 1);

            yaw += deltaX;
            pitch += deltaY;
            pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

            cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }


        private void UpdateZoom()
        {
            float targetFOV = Mathf.Lerp(maxZoomFOV, minZoomFOV, zoomValue);
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * zoomSmoothSpeed);
        }

        public void SetZoomValue(float value)
        {
            zoomValue = Mathf.Clamp01(value);
            OnZoomChanged?.Invoke(zoomValue);
        }
    }
}
