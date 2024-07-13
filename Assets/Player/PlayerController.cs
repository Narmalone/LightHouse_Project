using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 6.0f;
    public float jumpHeight = 1.0f;
    public float gravity = -9.81f;

    private Vector3 velocity;
    private bool isGrounded;
    private Vector2 moveInput;

    private void OnEnable()
    {
        // Assurez-vous que l'Input Action Asset est activé
        var inputAction = new PIA();
        inputAction.Enable();
        inputAction.Game.Move.performed += OnMove;
        inputAction.Game.Move.canceled += OnMove;
        inputAction.Game.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        var inputAction = new PIA();
        inputAction.Disable();
        inputAction.Game.Move.performed -= OnMove;
        inputAction.Game.Move.canceled -= OnMove;
        inputAction.Game.Jump.performed -= OnJump;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * Time.deltaTime * speed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

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
}
