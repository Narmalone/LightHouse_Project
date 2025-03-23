using TMPro;
using UnityEngine;

namespace LightHouse.Inventory
{
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemName_TMP;
        [SerializeField] private TextMeshProUGUI _itemUseKey_TMP;
        public TextMeshProUGUI ItemName_TMP => _itemName_TMP;
        public TextMeshProUGUI ItemUseKey_TPM => _itemUseKey_TMP;

        private IInventoryItem _inventoryItem;
        public IInventoryItem InventoryItem => _inventoryItem;

        private IInventoryItemUsable _inventoryItemUsable;
        private IInventoryItemCallback _inventoryItemCallback;

        public IInventoryItemCallback InventoryItemCallback => _inventoryItemCallback;
        public IInventoryItemUsable InventoryItemUsable => _inventoryItemUsable;

        public void SetInventoryItem(IInventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
        }

        public void SetItemCallback(IInventoryItemCallback inventoryItemCallback) 
        {
            _inventoryItemCallback = inventoryItemCallback;
        }

        public void SetItemUsable(IInventoryItemUsable inventoryItemUsable)
        {
            _inventoryItemUsable = inventoryItemUsable;
        }

        public void ResetSlot()
        {
            if(_inventoryItem != null)
                _inventoryItem = null;

            if(_inventoryItemUsable != null)
                _inventoryItemUsable = null;

            if(_inventoryItemCallback != null)
                _inventoryItemCallback = null;
        }
    }
}

