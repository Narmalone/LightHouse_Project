using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform cameraHolder; // Référence à un objet qui tient la caméra
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private Vector2 rotation = Vector2.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Cache le curseur
    }

    void Update()
    {
        
    }
}
