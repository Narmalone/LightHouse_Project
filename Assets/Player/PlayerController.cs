using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;
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

    private void OnEnable()
    {
        var inputAction = new PIA();
        inputAction.Enable();
        inputAction.Game.Move.performed += OnMove;
        inputAction.Game.Move.canceled += OnMove;
        inputAction.Game.Jump.performed += OnJump;
        inputAction.Game.Look.performed += OnLook;
        inputAction.Game.Look.canceled += OnLook;
    }

    private void OnDisable()
    {
        var inputAction = new PIA();
        inputAction.Disable();
        inputAction.Game.Move.performed -= OnMove;
        inputAction.Game.Move.canceled -= OnMove;
        inputAction.Game.Jump.performed -= OnJump;
        inputAction.Game.Look.performed -= OnLook;
        inputAction.Game.Look.canceled -= OnLook;
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
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
    }

    #endregion
}
