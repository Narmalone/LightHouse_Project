using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    public CharacterController controller;
    public Camera playerCamera;
    public LayerMask itemMask;
    public float speed = 6.0f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    private Vector3 velocity;
    private bool isGrounded;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float rotationX = 0;

    private PIA playerInputs;
    private void OnEnable()
    {
        playerInputs = new PIA();
        playerInputs.Enable();
        playerInputs.Game.Move.performed += OnMove;
        playerInputs.Game.Move.canceled += OnMove;
        playerInputs.Game.Jump.performed += OnJump;
        playerInputs.Game.Look.performed += OnLook;
        playerInputs.Game.Look.canceled += OnLook;
    }

    private void OnDisable()
    {
        playerInputs.Disable();
        playerInputs.Game.Move.performed -= OnMove;
        playerInputs.Game.Move.canceled -= OnMove;
        playerInputs.Game.Jump.performed -= OnJump;
        playerInputs.Game.Look.performed -= OnLook;
        playerInputs.Game.Look.canceled -= OnLook;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        HandleMove();
        controller.Move(velocity * Time.deltaTime);
        HandleGravity();
        HandleLook();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, itemMask))
        {
            Debug.DrawRay(playerCamera.transform.position, hit.point - playerCamera.transform.position);
            /*hit.collider.gameObject.TryGetComponent(out IItem item);
            if(item != null)
            {
                
            }*/
            //Debug.Log(item.ToString());
        }
    }

    #region INPUTS DELEGATES

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    #endregion

    #region HANDLING FUNCTIONS

    private void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleMove()
    {
        controller.Move(transform.right * moveInput.x * Time.deltaTime * speed);
        controller.Move(transform.forward * moveInput.y * Time.deltaTime * speed);
    }

    private void HandleLook()
    {
        rotationX += -lookInput.y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
    }

    #endregion
}
