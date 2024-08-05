using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask _itemMask;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private CustomEvent_IItem _DisplaySelection;
    [SerializeField] private CustomEvent _HideSelection;

    private PlayerManager _manager;
    private Camera playerCamera;
    private Transform playerCameraTransform;

    private IItem currentHitItem;

    public static Action PutInInventory;
    private PIA playerInputs;

    private void Awake()
    {
        playerInputs = new PIA();
        playerInputs.Enable();

        playerInputs.Game.Pickup.performed += OnInventoryGrab;
        playerInputs.Game.Interact.performed += OnInteract;
    }
    
    private void OnDestroy()
    {
        playerInputs.Game.Pickup.performed -= OnInventoryGrab;
        playerInputs.Game.Interact.performed -= OnInteract;
    }

    void Update()
    {
        HandleInteraction();
    }

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
        playerCamera = manager._data.playerCamera;
        playerCameraTransform = playerCamera.transform;
    }


    #region HANDLES

    private void HandleInteraction()
    {
        if (playerCameraTransform == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        // Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * _raycastDistance, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _itemMask))
        {
            Debug.DrawRay(playerCamera.transform.position, hit.point - playerCamera.transform.position);
            hit.collider.gameObject.TryGetComponent(out IItem item);
            if (item != null)
            {
                Select(item);
            }
            else
            {
                UnSelect();
            }
        }
        else
        {
            UnSelect();
        }
    }

    private void OnInventoryGrab(InputAction.CallbackContext context)
    {
        if (currentHitItem == null || currentHitItem.IsInventoryItem == false) return;

        currentHitItem.go.TryGetComponent(out ItemBase item);

        if (item == null) return;

        PlayerInventory.TakeItemAction.Invoke(item);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (currentHitItem == null || currentHitItem.IsUsable == false) return;
        currentHitItem.go.TryGetComponent(out ItemBase item);

        if (item == null) return;

        item.Use();
    }

    private void Select(IItem item)
    {
        if (currentHitItem != item)
        {
            currentHitItem = item;

            _DisplaySelection.Raise(currentHitItem);
        }
    }

    private void UnSelect()
    {
        if (currentHitItem != null)
        {
            currentHitItem = null;
        }

        _HideSelection.Raise();
    }

    #endregion
}
