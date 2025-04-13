using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Inventory
{
    public class InventoryUIController : MonoBehaviour
    {
        #region FIELDS
        [Header("References")]
        [SerializeField] private ItemSlot _slotPrefab;
        [SerializeField] private RectTransform _inventoryLayoutGroup;
        [SerializeField] private Image _holdItemImage;
        private ItemSlot[] _generatedSlots;
        #endregion

        #region GENERATE
        public ItemSlot[] GenerateItemSlot(int numberOfSlots, ItemDatabase database)
        {
            _generatedSlots = new ItemSlot[numberOfSlots];
            for (int i = 0; i < _generatedSlots.Length; i++)
            {
                _generatedSlots[i] = Instantiate(_slotPrefab, _inventoryLayoutGroup);
                _generatedSlots[i].SlotDatas.SlotID = (byte)i;
                _generatedSlots[i].Init(database);
            }
            return _generatedSlots;
        }
        #endregion

        #region InteractItemInInventory Fill Image
        public void FillHoldedImage(float fillValue)
        {
            if (_holdItemImage.fillAmount == fillValue) return;
            _holdItemImage.fillAmount = fillValue;
        }
        #endregion
    }
}

