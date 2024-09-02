using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public enum KeyMapCustom
{
    Move,
    Look, 
    Jump,
    Sprint,
    UseItem
}
public class PlayerController : MonoBehaviour
{
    [Header("Layer")]
    public LayerMask _layerWall;
    public LayerMask itemMask;

    [Header("Lock")]
    public bool lockMovements = false;
    public bool lockCameraMovements = false;


    [Header("Movements")]
    public float speed = 6.0f;
    public float _sprintSpeed = 6.0f;
    public float _crouchspeed = 3f;
    public float _crouchHeight = 1.25f;
    public float _crouchCameraHeight = 0;
    public float _crouchCenter = .39f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public float _heightCheckWallAbove = 1f;
    public float _rationSpeedWhenGrabbing = 1;

    [Header("Other")]
    public float raycastDistance = 3f;

    public PlayerManager _manager;
    private CharacterController controller;
    private PlayerInventory playerInventory;
    private Transform playerCamera;
    private Vector3 velocity;
    private Vector3 _initialCameraPosition;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private bool _isSprinting;
    private bool _isCrouching;
    private float _initialHeight;
    private float _initialCenter;
    private float rotationX = 0;
    private float _initialSpeed = 0;

    private PIA playerInputs;

    private IItem currentHitItem;

    private Coroutine _coroutineUncrouch;

    private void Awake()
    {
        playerInputs = new PIA();
        playerInputs.Enable();

        playerInputs.Game.Move.performed += OnMove;
        playerInputs.Game.Move.canceled += OnMove;
        playerInputs.Game.Crouch.performed += OnCrouch;
        playerInputs.Game.Crouch.canceled += OnCrouch;
        playerInputs.Game.Sprint.performed += OnSprint;
        playerInputs.Game.Sprint.canceled += OnSprint;
        playerInputs.Game.Jump.performed += OnJump;
        playerInputs.Game.Look.performed += OnLook;
        playerInputs.Game.Look.canceled += OnLook;
    }

    private void Start()
    {
        _manager._data._eventLockCameraMovement.handle += OnLockCamera;
        _manager._data._eventUnlockCameraMovement.handle += OnUnlockCamera;
        _manager._data._eventLockMovement.handle += OnLockMovement;
        _manager._data._eventUnlockMovement.handle += OnUnlockMovement;

        _manager._eventUpdate += OnUpdate;
    }

    private void OnDestroy()
    {
        playerInputs.Disable();

        playerInputs.Game.Move.performed -= OnMove;
        playerInputs.Game.Move.canceled -= OnMove;
        playerInputs.Game.Crouch.performed -= OnCrouch;
        playerInputs.Game.Crouch.canceled -= OnCrouch;
        playerInputs.Game.Sprint.performed -= OnSprint;
        playerInputs.Game.Sprint.canceled -= OnSprint;
        playerInputs.Game.Jump.performed -= OnJump;
        playerInputs.Game.Look.performed -= OnLook;
        playerInputs.Game.Look.canceled -= OnLook;

        _manager._data._eventLockCameraMovement.handle -= OnLockCamera;
        _manager._data._eventUnlockCameraMovement.handle -= OnUnlockCamera;
        _manager._data._eventLockMovement.handle -= OnLockMovement;
        _manager._data._eventUnlockMovement.handle -= OnUnlockMovement;

        _manager._eventUpdate -= OnUpdate;
    }

    private void OnUpdate()
    {
        HandleMove();
        controller.Move(velocity * Time.deltaTime);
        HandleGravity();
        HandleLook();
    }


    public void Initialize(PlayerManager manager)
    {
        _manager = manager;

        controller = manager._data.controller;
        //optionController = manager._data.optionController;
        playerInventory = manager._data.playerInventory;
        playerCamera = manager._data.playerCamera;

        _initialSpeed = speed;
        _initialCameraPosition = playerCamera.localPosition;
        _initialHeight = controller.height;
        _initialCenter = controller.center.y;

    }

    private bool CheckWallAbove()
    {
        //Debug.DrawRay(transform.position + controller.height / 2 * Vector3.up, Vector3.up * _heightCheckWallAbove, Color.red);
        var startPoint = transform.position + controller.height / 2 * Vector3.up;
        return Physics.CheckCapsule(startPoint, startPoint + Vector3.up * _heightCheckWallAbove, controller.radius, _layerWall);
    }


    #region INPUTS DELEGATES

    private void OnSprint(InputAction.CallbackContext context)
    {
        if (_isCrouching)
        {
            _isSprinting = false;
            return;
        }

        _isSprinting = context.performed;
        HandleSprintSpeed();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed == false)
        {
            if (HandleUncrounch() == false)
            {
                HandleAutomaticUncrouch();
                return;
            }
        }

        if (lockMovements || _manager.Freeze) return;

        _isCrouching = context.performed;

        HandleCrouching();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (lockMovements || _manager.Freeze)
        {
            moveInput = Vector2.zero;
            return;
        }
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (lockMovements || _manager.Freeze) return;

        if (isGrounded)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        if (lockCameraMovements || _manager.Freeze) 
        {
            lookInput = Vector2.zero;
            return; 
        }
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLockMovement()
    {
        lockMovements = true;
    }

    private void OnUnlockMovement()
    {
        lockMovements = false;
    }

    private void OnUnlockCamera()
    {
        lockCameraMovements = false;
    }

    private void OnLockCamera()
    {
        lockCameraMovements = true;
    }

    #endregion

    #region HANDLING FUNCTIONS

    private void HandleSprintSpeed()
    {
        speed = _isSprinting ? _sprintSpeed : _initialSpeed;
    }
    private void HandleCrouching()
    {
        if (_isCrouching)
        {
            controller.height = _crouchHeight;
            controller.center = _crouchCenter * Vector3.up;
            playerCamera.localPosition = _crouchCameraHeight * Vector3.up;
            speed = _crouchspeed;
            return;
        }
        controller.height = _initialHeight;
        controller.center = _initialCenter * Vector3.up;
        playerCamera.localPosition = _initialCameraPosition;
        speed = _initialSpeed;
    }

    private bool HandleUncrounch()
    {
        //Debug.DrawLine(transform.position + _initialHeight / 2 * Vector3.up, transform.position + _initialHeight / 2 * Vector3.up + _initialHeight / 2 * Vector3.up * _heightCheckWallAbove, Color.red, 10);
        var startPoint = transform.position + _initialHeight / 2 * Vector3.up;
        return !Physics.CheckCapsule(startPoint, startPoint + _initialHeight / 2 * Vector3.up * _heightCheckWallAbove, controller.radius, _layerWall);
    }

    private void HandleAutomaticUncrouch()
    {
        if (_coroutineUncrouch != null) StopCoroutine(_coroutineUncrouch);
        _coroutineUncrouch = StartCoroutine(Uncrouch_Coroutine());
    }

    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (CheckWallAbove())
        {
            velocity.y = -1;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleMove()
    {
        if(lockMovements) return;

        var speedX = moveInput.x * Time.deltaTime * speed * _rationSpeedWhenGrabbing;
        var speedY = moveInput.y * Time.deltaTime * speed * _rationSpeedWhenGrabbing;

        controller.Move(transform.right * speedX);
        controller.Move(transform.forward * speedY);
    }

    private void HandleLook()
    {
        if (lockCameraMovements) return;

        rotationX += -lookInput.y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
    }

    #endregion

    #region Coroutine

    IEnumerator Uncrouch_Coroutine()
    {
        bool canUncrouch = false;
        while (canUncrouch == false)
        {
            var startPoint = transform.position + _initialHeight / 2 * Vector3.up;
            canUncrouch = !Physics.CheckCapsule(startPoint, startPoint + _initialHeight / 2 * Vector3.up * _heightCheckWallAbove, controller.radius, _layerWall);
            yield return null;
        }

        _isCrouching = false;
        HandleCrouching();
    }
    #endregion
}