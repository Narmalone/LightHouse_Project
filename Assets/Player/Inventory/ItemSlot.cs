using TMPro;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _itemName_TMP;
    [SerializeField] private TextMeshProUGUI _itemUseKey_TMP;
    public TextMeshProUGUI ItemName_TMP => _itemName_TMP;
    public TextMeshProUGUI ItemUseKey_TPM => _itemUseKey_TMP;

    private IInventoryItem _inventoryItem;
    public IInventoryItem InventoryItem => _inventoryItem;

    public void SetInventoryItem(IInventoryItem inventoryItem)
    {
        _inventoryItem = inventoryItem;
    }

    public void ResetSlot()
    {
        _inventoryItem = null;
    }
}
