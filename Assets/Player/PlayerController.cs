using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public ItemOptionController optionController;
    public PlayerInventory playerInventory;
    public Camera playerCamera;
    public LayerMask _layerWall;
    public LayerMask itemMask;
    public float speed = 6.0f;
    public float _sprintSpeed = 6.0f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public float _heightCheckWallAbove = 1f;

    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private bool _isSprinting;
    private bool _isCrouching;
    private float rotationX = 0;
    private float _initialSpeed = 0;

    private PIA playerInputs;

    public float raycastDistance = 3f;
    private IItem currentHitItem;

    private void Awake()
    {
        _initialSpeed = speed;

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
    }

    private void Update()
    {
        HandleMove();
        controller.Move(velocity * Time.deltaTime);
        HandleGravity();
        HandleLook();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, itemMask))
        {
            Debug.DrawRay(playerCamera.transform.position, hit.point - playerCamera.transform.position);
            hit.collider.gameObject.TryGetComponent(out IItem item);
            if (item != null)
            {
                if (currentHitItem != item)
                {
                    currentHitItem = item;
                    OnRaycastEnter(item);
                }
            }
            else
            {
                if (currentHitItem != null)
                {
                    OnRaycastExit(currentHitItem);
                    currentHitItem = null;
                }
            }
        }
        else
        {
            if (currentHitItem != null)
            {
                OnRaycastExit(currentHitItem);
                currentHitItem = null;
            }
        }

    }

    private void OnRaycastEnter(IItem item)
    {
        //Debug.Log("Raycast entered: " + item.Name);
        ShowOptions(item);
    }

    private void OnRaycastExit(IItem item)
    {
        //Debug.Log("Raycast exited: " + item.Name);
        HideOptions();
    }

    private void ShowOptions(IItem item)
    {
        optionController.Show();
        optionController.ItemName.text = item.ItemDatas.itemName;
        List<GameObject> temp = new List<GameObject>();
        foreach (var option in item.GetOptions())
        {
            var buttonObject = optionController.AddButton();
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = option.Name;

            // Add a listener to the button's click event
            if(option is GrabOptionBase grabOption)
            {

            }
            else if (option is HoldOptionBase holdOption)
            {
                buttonObject.onClick.AddListener(() =>
                {
                    playerInventory.AddItemToInventory(item.go, item.ItemDatas);
                    item.go.SetActive(false);
                    Debug.Log("additem");
                });
            }
            else if (option is UseOptionBase useOption)
            {
                buttonObject.onClick.AddListener(() =>
                {
                    useOption.UseAction?.Invoke();
                });
            }
            temp.Add(buttonObject.gameObject);
        }
        EventSystem.current.SetSelectedGameObject(temp[0]);
    }

    private void HideOptions()
    {
        optionController.ClearButtons();
        optionController.Hide();
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
        _isSprinting = context.performed;

        speed = _isSprinting ? _sprintSpeed: _initialSpeed;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        _isCrouching = context.performed;

        // down the collider of the player
        // down the camera of the player
        // Slow the speed
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

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    #endregion

    #region HANDLING FUNCTIONS

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
