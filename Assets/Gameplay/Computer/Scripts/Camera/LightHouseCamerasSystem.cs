using System;
using UnityEngine;

public class LightHouseCamerasSystem : MonoBehaviour
{
    public LightHouseCameraController[] Cameras;
    public static event Action OnCameraModeEnabled;
    public static event Action OnCameraModeDisabled;

    public void EnableCameraMode()
    {
        Cameras[0].SetEnable(true);
        OnCameraModeEnabled?.Invoke();
    }


    public void DisableCameraMode()
    {
        Cameras[0].SetEnable(false);
        OnCameraModeDisabled?.Invoke();
    }
}
