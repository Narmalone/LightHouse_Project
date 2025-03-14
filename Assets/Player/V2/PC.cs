using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PC : MonoBehaviour
{
    public PIA PlayerInputs;
    public float BaseSpeed = 5.0f;
    public float Gravity = -9.81f;

    public Vector3 Velocity = Vector3.zero;
    public Vector2 MoveInput;
    public Vector2 LookInput;

    private CharacterController charaController;

    public bool IsGrounded = false;

    private void Awake()
    {
        InitInputs();
    }

    private void OnValidate()
    {
        charaController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        IsGrounded = charaController.isGrounded;
        HandleGravity(Gravity);
        Debug.Log(charaController.isGrounded);
    }

    public void HandleGravity(float gravity)
    {
        if (!IsGrounded)
        {
            Velocity.y += Gravity * Time.deltaTime;

        }
        else
        {
            Velocity.y = 0f;
        }
    }


    public void InitInputs()
    {
        PlayerInputs = new PIA();
        PlayerInputs.Enable();

        RegisterInputCallback();
    }

    public void RegisterInputCallback()
    {
        //performed
        PlayerInputs.Game.Move.performed += Move_performed;
        PlayerInputs.Game.Look.performed += Look_performed;

        //canceled
        PlayerInputs.Game.Move.canceled += Move_canceled;
    }

    private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(obj.ReadValue<Vector2>() == Vector2.zero)
            MoveInput = Vector2.zero;
    }

    private void OnDestroy()
    {
        PlayerInputs.Game.Move.performed -= Move_performed;
        PlayerInputs.Game.Look.performed -= Look_performed;

        PlayerInputs.Game.Move.canceled -= Move_canceled;
    }

    private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        MoveInput = obj.ReadValue<Vector2>();
    }

    private void Look_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LookInput = obj.ReadValue<Vector2>();
    }
}
