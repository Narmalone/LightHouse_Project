using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Inventory
{
    public class InventoryUIController : MonoBehaviour
    {
        #region FIELDS
        [Header("References")]
        [SerializeField] private CanvasGroup _inventoryCanvasGroup;
        [SerializeField] private ItemSlot _slotPrefab;
        [SerializeField] private RectTransform _inventoryLayoutGroup;
        [SerializeField] private Image _holdItemImage;
        [SerializeField] private Image _dropHoldPowerImage;
        private ItemSlot[] _generatedSlots;
        #endregion

        #region Public API

        /// <summary>
        /// Montre visuellement les slots de l'inventaire mais n'active pas l'interaction avec le scroll
        /// </summary>
        public void Show()
        {
            _inventoryCanvasGroup.alpha = 1.0f;
        }

        /// <summary>
        /// Montre visuellement les slots de l'inventaire mais ne désactive pas l'interaction avec le scroll
        /// </summary>
        public void Hide()
        {
            _inventoryCanvasGroup.alpha = 0.0f;
        }

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
        public void FillInteractHoldedImage(float fillValue)
        {
            if (_holdItemImage.fillAmount == fillValue) return;
            _holdItemImage.fillAmount = fillValue;
        }
        #endregion

        #region Drop Power Fill Image
        public void FillDropHoldedImage(float fillValue)
        {
            if (_dropHoldPowerImage.fillAmount == fillValue) return;
            _dropHoldPowerImage.fillAmount = fillValue;
        }
        #endregion
    }
}

