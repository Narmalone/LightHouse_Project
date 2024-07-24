using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public float raycastDistance = 3f;
    private IItem currentHitItem;
    private GameObject optionsPanel;
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
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, itemMask))
        {
            Debug.DrawRay(playerCamera.transform.position, hit.point - playerCamera.transform.position);
            hit.collider.gameObject.TryGetComponent(out IItem item);
            if (item != null)
            {
                if (currentHitItem != item)
                {
                    OnRaycastEnter(item);
                    currentHitItem = item;
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

    void OnRaycastEnter(IItem item)
    {
        Debug.Log("Raycast entered: " + item.Name);
        ShowOptions(item);
    }

    void OnRaycastExit(IItem item)
    {
        Debug.Log("Raycast exited: " + item.Name);
        HideOptions();
    }

    void ShowOptions(IItem item)
    {
        // Create a UI panel to display the options
        optionsPanel = new GameObject("Options Panel");
        optionsPanel.transform.SetParent(GameObject.Find("Canvas").transform);

        // Set the panel's anchor and pivot to the bottom left corner
        RectTransform panelRect = optionsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 0);
        panelRect.pivot = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(10, 10); // adjust the position as needed

        // Create a UI button for each option
        foreach (var option in item.GetOptions())
        {
            // Create a UI button
            GameObject buttonObject = new GameObject("Button");
            buttonObject.transform.SetParent(optionsPanel.transform);

            // Set the button's anchor and pivot to the top left corner
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(0, 1);
            buttonRect.pivot = new Vector2(0, 1);
            buttonRect.sizeDelta = new Vector2(200, 30); // adjust the size as needed

            // Add a Button component to the button object
            Button button = buttonObject.AddComponent<Button>();
            Image img = buttonObject.AddComponent<Image>();
            button.targetGraphic = img;

            // Set the button's text to the option's name
            Text buttonText = new GameObject("Button Text").AddComponent<Text>();
            buttonText.transform.SetParent(buttonObject.transform);
            buttonText.text = option.Name;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontSize = 18; // adjust the font size as needed

            // Add a listener to the button's click event
            if (option is UseOptionBase useOption)
            {
                button.onClick.AddListener(() =>
                {
                    useOption.UseAction?.Invoke();
                });
            }
        }
    }

    void HideOptions()
    {
        if (optionsPanel != null)
        {
            Destroy(optionsPanel);
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
