using UnityEngine;

public class PlayerControllerMenu : MonoBehaviour
{
    private GameObject _lastHittedObject;
    private RaycastableMenuItem _lastHittedItem;

    private RaycastableMenuItem _selectedItem; // 👈 IMPORTANT

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 25f))
        {
            HandleHit(hit);
        }
        else
        {
            ClearCurrentItem();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (_lastHittedItem == null)
            return;

        // Si on clique sur un autre → on cache l'ancien
        if (_selectedItem != null && _selectedItem != _lastHittedItem)
        {
            _selectedItem.HideInformations();
        }

        if (_selectedItem == _lastHittedItem)
        {
            _selectedItem.HideInformations();
            _selectedItem = null;
            return;
        }

        _selectedItem = _lastHittedItem;
        _selectedItem.ShowInformations();

        Debug.Log($"Clicked on {_selectedItem.name}");
    }

    private void HandleHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        if (_lastHittedObject == hitObject)
            return;

        if (_lastHittedItem != null)
        {
            _lastHittedItem.OnRaycastExit();
            _lastHittedItem = null;
            _lastHittedObject = null;
        }

        _lastHittedObject = hitObject;

        RaycastableMenuItem menuItem = hit.collider.GetComponent<RaycastableMenuItem>();
        if (menuItem != null)
        {
            _lastHittedItem = menuItem;
            _lastHittedItem.OnRaycastEnter();
        }
    }

    private void ClearCurrentItem()
    {
        if (_lastHittedItem != null)
        {
            _lastHittedItem.OnRaycastExit();
            _lastHittedItem = null;
            _lastHittedObject = null;
        }
    }
}