using LightHouse.Features.Items.Inventory.Databases;
using LightHouse.Features.Items.Inventory.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Core.Player.Inventory.UI
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
        public List<ItemSlot> GenerateItemSlot(int numberOfSlots, ItemDatabase database)
        {
            List<ItemSlot> generatedSlots = new List<ItemSlot>();
            for (int i = 0; i < numberOfSlots; i++)
            {
                ItemSlot newSlot = Instantiate(_slotPrefab, _inventoryLayoutGroup);
                generatedSlots.Add(newSlot);
                SlotManager.AddSlot(newSlot);
                newSlot.Init(database, (byte)(SlotManager.SlotLength - 1));
            }
            return generatedSlots;
        }

        public List<ItemSlot> AddItemToSlots(int slotsToAdd, ItemDatabase database)
        {
            SlotManager.AddSlots(GenerateItemSlot(slotsToAdd, database));
            return SlotManager.Slots;
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

