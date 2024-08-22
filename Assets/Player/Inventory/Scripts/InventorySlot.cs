using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage; // Reference to the icon image component
    public TextMeshProUGUI itemName; // Reference to the icon image component
    public IItem item; // The item currently in this slot
    public GameObject border; // Reference to the 3D preview object
    public ItemBase previewItem;
    public bool isSelected = false; // Is this slot currently selected?
    public bool isEmpty => item == null;

    public bool IsClickable = false;

    [SerializeField] private CustomEvent _onStorageItemOpen;
    [SerializeField] private CustomEvent_ItemBase _fromInventoryToStorage;
    [SerializeField] private CustomEvent_InventorySlot _sendItemToStorageFromSlot;
    [SerializeField] private CustomEvent _eventDropItem;

    private void Awake()
    {
        _onStorageItemOpen.handle += _onStorageItemOpen_handle;
    }

    private void _onStorageItemOpen_handle()
    {
        IsClickable = true;
    }

    private void Start()
    {
        itemName.alpha = 0;
    }

    private void OnDestroy()
    {
        _onStorageItemOpen.handle -= _onStorageItemOpen_handle;
    }

    public void SetItem(IItem newItem)
    {
        item = newItem;
        UpdateDisplay();
    }

    public void SetPreviewItem(ItemBase item)
    {
        previewItem = item;
        if (previewItem == null) return;
        previewItem.TryGetComponent(out Rigidbody rb); 
        previewItem._collider.enabled = false;
        previewItem.isInventoryItem = false;
        if (rb == null) return;
        rb.isKinematic = true;
    }

    public void RaiseUseItem()
    {
        if (previewItem.isUsable == false) return;
        bool isDestroy = previewItem.Use();
        if (isDestroy) _eventDropItem.Raise();
    }

    public void UpdateDisplay()
    {
        if (item != null)
        {
            // Display the icon
            iconImage.sprite = item.ItemDatas.icon;
            itemName.text = item.Name;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsClickable || previewItem == null) return;
        if(eventData.clickCount >= 2)
        {
            //Set preview item & remove celui là
            _fromInventoryToStorage?.Raise(previewItem);
            _sendItemToStorageFromSlot?.Raise(this, previewItem);
        }
    }
}