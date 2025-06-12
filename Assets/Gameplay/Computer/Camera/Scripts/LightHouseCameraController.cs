using Cinemachine;
using LightHouse.Inputs;
using UnityEngine;

public class LightHouseCameraController : MonoBehaviour
{
    [Header("Camera")]
    public CinemachineVirtualCamera LightHouseCamera;
    public bool IsEnabled = false;
    public float sensitivity = 1.5f;
    public float verticalClamp = 80f;

    [Header("Input Axis")]
    public bool invertX = false;
    public bool invertY = true;

    [Header("Zoom Settings")]
    public float minZoomFOV = 25f;
    public float maxZoomFOV = 60f;
    public float zoomSpeed = 3f;

    private float yaw;
    private float pitch;
    private Transform cameraTarget;
    private float currentFOV;

    private void Start()
    {
        if (LightHouseCamera != null)
        {
            cameraTarget = LightHouseCamera.transform;
            currentFOV = LightHouseCamera.m_Lens.FieldOfView;
        }

        
    }

    public void SetEnable(bool value)
    {
        IsEnabled = value;

        if (IsEnabled)
            LightHouseCamera.Priority = 100;
        else
            LightHouseCamera.Priority = -1;
    }

    private void Update()
    {
        if (!IsEnabled) return;
        Vector2 lookInputs = InputManager.Player.Look.ReadValue<Vector2>();
        UpdateCameraRotation(lookInputs);

        if (InputManager.Player.CameraZoom.IsPressed())
        {
            ZoomCamera();
        }
        else
        {
            UnZoomCamera();
        }

        // Appliquer le FOV interpolé
        LightHouseCamera.m_Lens.FieldOfView = currentFOV;
    }

    public void ZoomCamera()
    {
        currentFOV = Mathf.Lerp(currentFOV, minZoomFOV, Time.deltaTime * zoomSpeed);
    }

    public void UnZoomCamera()
    {
        currentFOV = Mathf.Lerp(currentFOV, maxZoomFOV, Time.deltaTime * zoomSpeed);
    }

    public void UpdateCameraRotation(Vector2 lookInputs)
    {
        if (lookInputs == Vector2.zero || cameraTarget == null) return;

        float deltaX = lookInputs.x * sensitivity * (invertX ? -1 : 1);
        float deltaY = lookInputs.y * sensitivity * (invertY ? -1 : 1);

        yaw += deltaX;
        pitch += deltaY;
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
