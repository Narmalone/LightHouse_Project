using LightHouse.KinematicCharacterController;
using System;
using UnityEngine;

public class LightHouseCamerasSystem : MonoBehaviour
{
    public LightHouseCameraController[] Cameras;
    public int CurrentActiveCamera;

    public bool CameraModeEnabled = false;
    public static event Action OnCameraModeEnabled;
    public static event Action OnCameraModeDisabled;

    private void Awake()
    {
        CurrentActiveCamera = 0;
    }

    public void EnableCameraMode()
    {
        CameraModeEnabled = true;
        Player.ForceChangePlayerState.Invoke(PlayerState.CameraMode);
        Cameras[CurrentActiveCamera].SetEnable(true);
        OnCameraModeEnabled?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!CameraModeEnabled)
                EnableCameraMode();
            else DisableCameraMode();
        }
    }

    public void DisableCameraMode()
    {
        CameraModeEnabled = false;
        Cameras[CurrentActiveCamera].SetEnable(false);
        OnCameraModeDisabled?.Invoke();
        Player.ForceChangePlayerState.Invoke(PlayerState.Normal);
    }
}
