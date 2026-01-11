using LightHouse.Core.Player.Inventory;
using LightHouse.Features.Items.Inventory.Jerrican;
using System;

namespace LightHouse.Features.Items.Interactable
{
    public class JerricanItemTracker : IDUseItemTracker
    {
        protected JerricanEssence _currentJerricanOnhands;
        public event Action<float> OnJerricanUsed;

        protected override void Usable_OnItemUsed()
        {
            if (_inventoryItemUsable == null) return;
            _currentJerricanOnhands = _inventoryItemUsable as JerricanEssence;
            base.Usable_OnItemUsed();
            _currentJerricanOnhands.InvokeForceDropItemFromInventory(InventoryHandlerData.InventoryTargetPosition.position, 0.0f, false);
            OnJerricanUsed?.Invoke(_currentJerricanOnhands.EssenceAmount);
            Destroy(_currentJerricanOnhands.gameObject);
        }
    }

}
