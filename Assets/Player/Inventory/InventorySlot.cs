using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage; // Reference to the icon image component
    private InventoryItemData item; // The item currently in this slot
    public GameObject previewObject; // Reference to the 3D preview object
    public bool isSelected = false; // Is this slot currently selected?

    public void SetItem(InventoryItemData newItem)
    {
        item = newItem;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (item != null)
        {
            // Display the 3D preview
            previewObject.SetActive(true);
            previewObject.GetComponent<MeshFilter>().mesh = item.mesh;
            previewObject.GetComponent<MeshRenderer>().material = item.material;

            // Display the icon
            iconImage.sprite = item.icon;
        }
        else
        {
            // Clear the display
            previewObject.SetActive(false);
            iconImage.sprite = null;
        }
    }

    public void OnSelect()
    {
        isSelected = true;
        // Update the UI to reflect the selection
    }

    public void OnDeselect()
    {
        isSelected = false;
        // Update the UI to reflect the deselection
    }


}