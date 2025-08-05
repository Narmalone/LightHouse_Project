using LightHouse.Game.Computer.Cameras;
using LightHouse.Game.DayNightSystem;
using LightHouse.Inputs;
using LightHouse.Weather;
using LightHouse.Weather.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.UI
{
    /// <summary>
    /// UI logic for interacting with the camera system: zoom, fullscreen, render.
    /// </summary>
    public class UI_CameraView : LEOWindow
    {
        [Header("Camera System")]
        [SerializeField] private LightHouseCamerasSystem _cameraSystem;

        [Header("UI References")]
        [SerializeField] private RawImage _currentRenderCam;
        [SerializeField] private Slider _zoomSlider;
        [SerializeField] private Button _fullScreenToggle;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private RectTransform _compassTransform;

        [Header("Fullscreen")]
        [SerializeField] private FullscreenCameraView _fullscreenCameraViewPrefab;
        private FullscreenCameraView _fullScreenInstance;

        #region Unity Events

        private void Awake()
        {
            _zoomSlider.onValueChanged.AddListener(OnZoomValueChanged);
            _fullScreenToggle.onClick.AddListener(OnFullscreenClicked);
        }

        private void Start()
        {
            _fullScreenInstance = Instantiate(_fullscreenCameraViewPrefab, null);
            _fullScreenInstance.OnFullScreenButtonCliqued += HandleFullscreenExit;
            _fullScreenInstance.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            //
            string dayText = TimeUtility.FormatDay(TimeHandlerData.CurrentDay);
            _dayText.text = "/// " + dayText + " - " + TimeUtility.FormatTime12h(TimeHandlerData.CurrentTime);
            
            if(_fullScreenInstance.isActiveAndEnabled) _fullScreenInstance.DayText.text = _dayText.text;

            if(_cameraSystem != null)
                UpdateCompassByCameraDirection();
        }

        private void OnDestroy()
        {
            _zoomSlider.onValueChanged.RemoveListener(OnZoomValueChanged);
            _fullScreenToggle.onClick.RemoveListener(OnFullscreenClicked);

            if (_cameraSystem != null && _cameraSystem.CurrentActiveCamera != null)
            {
                _cameraSystem.CurrentActiveCamera.OnZoomChanged -= UpdateSlider;
            }

            if (_fullScreenInstance != null)
            {
                _fullScreenInstance.OnFullScreenButtonCliqued -= HandleFullscreenExit;
            }
        }

        #endregion

        #region Window Lifecycle

        public override void Open()
        {
            base.Open();
            _cameraSystem.EnableCameraMode();
        }

        public override void Close()
        {
            base.Close();
            TryCheckServices();
            _cameraSystem?.DisableCameraMode();
        }

        #endregion

        #region Fullscreen

        private void OnFullscreenClicked()
        {
            _cameraSystem.CurrentActiveCamera.SetEnableFullScren(true);
            _fullScreenInstance.SetTexture(_cameraSystem.CurrentActiveCamera.RenderTextureCamera.renderTexture);
            _fullScreenInstance.gameObject.SetActive(true);
        }

        private void HandleFullscreenExit()
        {
            _cameraSystem.CurrentActiveCamera.SetEnableFullScren(false);
            _fullScreenInstance.gameObject.SetActive(false);
        }

        #endregion

        #region Zoom

        private void OnZoomValueChanged(float value)
        {
            if (_cameraSystem.CameraModeEnabled)
            {
                _cameraSystem.CurrentActiveCamera.SetZoomValue(value);
            }
        }

        private void UpdateSlider(float value)
        {
            _zoomSlider.SetValueWithoutNotify(value); // Prevents feedback loop
        }

        #endregion

        /// <summary>
        /// Retourne l'angle horizontal de la caméra sur le plan XZ (0° = Nord, 90° = Est, etc.)
        /// </summary>
        private float GetCompassAngleFromCamera()
        {
            var camForward = _cameraSystem.CurrentActiveCamera.transform.forward;
            camForward.y = 0; // Ignore la composante verticale
            camForward.Normalize();

            float angle = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;
            return angle;
        }

        private void UpdateCompassByCameraDirection()
        {
            float angle = GetCompassAngleFromCamera();
            _compassTransform.localEulerAngles = new Vector3(0f, 0f, -angle);

            // Conversion de l’angle en direction
            WindOrientationType orientation = WeatherUtils.AngleToOrientationType(angle);
        }

        #region Initialization

        private void TryCheckServices()
        {
            if (OSSystem.Services.CameraSystem != null)
            {
                _cameraSystem = OSSystem.Services.CameraSystem;

                var currentCamera = _cameraSystem.CurrentActiveCamera;
                currentCamera.RenderTextureCamera.RenderOnce();
                _currentRenderCam.texture = _cameraSystem.GetCurrentCameraRenderTexture();

                currentCamera.OnZoomChanged += UpdateSlider;
                _zoomSlider.SetValueWithoutNotify(currentCamera.zoomValue);
            }
        }

        #endregion
    }
}
