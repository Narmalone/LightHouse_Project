using System;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage; // Reference to the icon image component
    public TextMeshProUGUI itemName; // Reference to the icon image component
    public ItemDatas item; // The item currently in this slot
    public GameObject border; // Reference to the 3D preview object
    public ItemBase previewItem;
    public bool isSelected = false; // Is this slot currently selected?
    public bool isEmpty => item == null;

    public void SetItem(ItemDatas newItem)
    {
        item = newItem;
        UpdateDisplay();
    }

    public void SetPreviewItem(ItemBase item)
    {
        previewItem = item;
        previewItem.TryGetComponent(out Rigidbody rb);
        if(rb == null) return;
        rb.isKinematic = true;
    }

    public void RaiseUseItem()
    {
        previewItem.Use();
    }

    public void UpdateDisplay()
    {
        if (item != null)
        {

            // Display the icon
            iconImage.sprite = item.icon;
            itemName.text = item.itemName;
        }
        else
        {
            iconImage.sprite = null;
            itemName.text = string.Empty;
        }
    }

    public void OnSelect()
    {
        isSelected = true;
        border.SetActive(true);
        itemName.alpha = 1;
        // Update the UI to reflect the selection
    }

    public void OnDeselect()
    {
        isSelected = false;
        border.SetActive(false);
        itemName.alpha = 0;
        // Update the UI to reflect the deselection
    }


}