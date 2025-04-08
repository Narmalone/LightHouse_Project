using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Inventory
{
    public class CanvasInventory : MonoBehaviour
    {
        [SerializeField] private ItemSlot _slotPrefab;
        [SerializeField] private RectTransform _inventoryLayoutGroup;
        [SerializeField] private Image _holdItemImage;
        public Image HoldItemProgressImage => _holdItemImage;
        private ItemSlot[] _generatedSlots;
        public ItemSlot[] GeneratedSlot => _generatedSlots;

        public ItemSlot[] GenerateItemSlot(int numberOfSlots)
        {
            _generatedSlots = new ItemSlot[numberOfSlots];
            for (int i = 0; i < _generatedSlots.Length; i++)
            {
                _generatedSlots[i] = Instantiate(_slotPrefab, _inventoryLayoutGroup);
                _generatedSlots[i].SlotDatas.SlodID = i;
            }
            return _generatedSlots;
        }

        public void DestroyAllSlots()
        {
            for (int i = 0; i < _generatedSlots.Length; i++)
            {
                Destroy(_generatedSlots[i]);
                _generatedSlots[i] = null;
            }
        }

        public void ResetAllSlots()
        {
            for (int i = 0; i < _generatedSlots.Length; i++)
            {
                _generatedSlots[i].ResetSlot();
            }
        }

        public void FillHoldedImage(float fillValue)
        {
            _holdItemImage.fillAmount = fillValue;
        }
    }
}

