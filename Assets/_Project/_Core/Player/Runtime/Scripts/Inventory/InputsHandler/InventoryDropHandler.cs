using LightHouse.Core.Services;
using LightHouse.Core.Inputs;
using LightHouse.Core.Audio;
using LightHouse.Features.Items.Inventory.UI;
using LightHouse.Core.Player.Inventory.UI;
using LightHouse.Features.Items.Inventory;

using UnityEngine;
using System.Collections.Generic;
using LightHouse.Core.Player.Inventory.Pool;
using LightHouse.Core.Player.Inventory.Callbacks;

namespace LightHouse.Core.Player.Inventory.InputsHandler
{
    public class InventoryDropHandler
    {
        #region FIELDS
        //DROP SETTINGS
        private float _maxDropPower = 10f;
        private AnimationCurve _dropPowerCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private float _securityOverlapSphereRadius = 0.3f;
        private LayerMask _securityObstacleMasks = 1 << 0;

        [Header("Debug only")]
        public Collider[] DropOverlappingColliders = new Collider[0];
        private List<ItemSlot> _slots => SlotManager.Slots;
        private Transform _inventoryTarget;
        private float _dropChargeTimer = 0f;
        private float _dropPower = 0f;
        private bool _isChargingDrop = false;
        private InventoryUIController _inventoryUIController;

        public bool IsChargingDrop => _isChargingDrop;
        #endregion

        public InventoryDropHandler(InventoryUIController inventoryUiController, Transform inventoryTargetDropTransform, float maxPower, AnimationCurve motionCurve, LayerMask securityMasks, float securityOverlapSphereRadius = 0.3f)
        {
            _maxDropPower = maxPower;
            _dropPowerCurve = motionCurve;
            _securityOverlapSphereRadius = securityOverlapSphereRadius;
            _securityObstacleMasks = securityMasks;
            _inventoryUIController = inventoryUiController;
            _inventoryTarget = inventoryTargetDropTransform;
        }

        #region Main Drop Function
        public void DropItem(int slotID, ushort itemGlobalID, ushort specificID, Vector3 pos, float force, bool enablePhysicsOnDrop, out IInventoryItem droppedItem)
        {
            droppedItem = null;
            if (!_slots[slotID].SlotDatas.HasItem) return;
            IInventoryItem item = InventoryPoolManager.Get(itemGlobalID, specificID, enablePhysicsOnDrop);
            droppedItem = item;

            Vector3 finalPos = GetAdjustedDropPosition(_inventoryTarget, pos);
            item.IsItemInInventory = false;


            if (enablePhysicsOnDrop)
            {
                Rigidbody rb = item.GetRigidBody();
                float appliedForce = (finalPos == pos) ? force : 0f;

                //IMPORTANT -> to force position, rotations and more for rigidbody we have to wait the next physic update
                PhysicsDropQueueSystem.QueueDrop(rb, finalPos, Quaternion.identity, _inventoryTarget.forward, appliedForce);
            }


            item.GetGameObject().transform.SetParent(null);
            _slots[slotID].RemoveItemFromSlot(item.ItemSpecificID);
            _slots[slotID].UnsubscribeToIInventoryItem(item);
            if (item is IInventoryItemCallback callback) callback.OnItemRemovedFromInventory();
            if(item is IInventoryItemUsable usable)
                _slots[slotID].UnsubscribeToIInventoryUsable(usable);

            InventoryHandlerData.NotifyDrop(slotID, item);
        }
        #endregion

        #region Check Func
        private Vector3 GetAdjustedDropPosition(Transform target, Vector3 intendedDropPosition, int lengthToSafePos = 0)
        {
            Vector3 start = target.position;
            float sphereRadius = _securityOverlapSphereRadius; // Ajuste la taille en fonction des objets

            // 1. V�rifier si un obstacle est proche avec un OverlapSphere
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            DropOverlappingColliders = colliders;
            if (colliders.Length > lengthToSafePos)
            {
                Vector3 adjustedPosition = PlayerHandlerData.MainPlayer.Character.transform.position + Vector3.up; 
                Debug.Log("Adjusting position due to overlap result");
                return adjustedPosition;
            }

            return intendedDropPosition;
        }
        #endregion

        #region MONO CALLBACKS
        public void OnDrawGizmos()
        {
            if (_inventoryTarget == null) return;
            Vector3 start = _inventoryTarget.position;
            float sphereRadius = _securityOverlapSphereRadius;
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            DropOverlappingColliders = colliders;
            Gizmos.color = DropOverlappingColliders.Length > 0 ? Color.red : Color.green;
            Gizmos.DrawSphere(_inventoryTarget.position, sphereRadius);
        }
        #endregion

        #region Input Handler
        public void HandleDropInput(bool playSound = true, SO_AudioCue soundToPlay = null)
        {
            if (InputManager.Drop.IsPressed())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                    return;

                _isChargingDrop = true;
                _dropChargeTimer += Time.deltaTime;
                float curveValue = _dropPowerCurve.Evaluate(Mathf.Clamp01(_dropChargeTimer));
                _dropPower = curveValue * _maxDropPower;
                _inventoryUIController.FillDropHoldedImage(curveValue);
            }
            else if (_isChargingDrop && InputManager.Drop.WasReleasedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                    return;
                DropItem
                (
                    slotID: SlotManager.CurrentSelectedSlot.SlotDatas.SlotID,
                    itemGlobalID: (ushort)SlotManager.CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                    specificID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0],
                    pos: _inventoryTarget.position,
                    force: _dropPower,
                    enablePhysicsOnDrop: true,
                    droppedItem: out IInventoryItem droppedItem
                );

                _dropPower = 0f;
                _dropChargeTimer = 0f;
                _isChargingDrop = false;
                if(playSound && soundToPlay != null && ServiceLocator.Audio != null)
                    ServiceLocator.Audio.PlayAt(soundToPlay, _inventoryTarget.position);
                _inventoryUIController.FillDropHoldedImage(0f);
            }
            else if (InputManager.Drop.WasPerformedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                    return;
                DropItem
                (
                    slotID: SlotManager.CurrentSelectedSlot.SlotDatas.SlotID,
                    itemGlobalID: (ushort)SlotManager.CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                    specificID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0],
                    pos: _inventoryTarget.position,
                    force: 0.0f,
                    enablePhysicsOnDrop: true,
                    droppedItem: out IInventoryItem droppedItem
                );
                if (playSound && soundToPlay != null && ServiceLocator.Audio != null)
                    ServiceLocator.Audio.PlayAt(soundToPlay, _inventoryTarget.position);
                _inventoryUIController.FillDropHoldedImage(0f);
            }
        }

        public void CancelDrop()
        {
            _dropPower = 0f;
            _dropChargeTimer = 0f;
            _isChargingDrop = false;
            _inventoryUIController.FillDropHoldedImage(0f);
        }
        #endregion

    }
}
