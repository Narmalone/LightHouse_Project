using System;
using UnityEngine;

namespace LightHouse.Features.Computer.LEO.Cameras
{
    public class LightHouseCamerasSystem : MonoBehaviour
    {
        public LightHouseCameraController[] Cameras;
        public int CurrentActiveCameraIndex;

        public LightHouseCameraController CurrentActiveCamera => Cameras[CurrentActiveCameraIndex];

        public bool CameraModeEnabled = false;
        public static event Action OnCameraModeEnabled;
        public static event Action OnCameraModeDisabled;

        private void Awake()
        {
            CurrentActiveCameraIndex = 0;
        }

        public void EnableCameraMode()
        {
            CameraModeEnabled = true;
            Cameras[CurrentActiveCameraIndex].SetEnable(true);
            OnCameraModeEnabled?.Invoke();
        }

        public void DisableCameraMode()
        {
            CameraModeEnabled = false;
            Cameras[CurrentActiveCameraIndex].SetEnable(false);
            OnCameraModeDisabled?.Invoke();
        }

        public RenderTexture GetCurrentCameraRenderTexture()
        {
            return Cameras[CurrentActiveCameraIndex].RenderTextureCamera.renderTexture;
        }

        public void SwitchCamera(int cameraIndex)
        {
            Cameras[CurrentActiveCameraIndex].SetEnable(false);
            CurrentActiveCameraIndex = cameraIndex;
            Cameras[CurrentActiveCameraIndex].SetEnable(true);
        }
    }
}

