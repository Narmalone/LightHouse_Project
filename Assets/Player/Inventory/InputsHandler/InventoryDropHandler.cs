using LightHouse.Handlers;
using LightHouse.Inputs;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace LightHouse.Inventory
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
        private ItemSlot[] _slots => SlotManager.Slots;
        private Transform _inventoryTarget;
        private float _dropChargeTimer = 0f;
        private float _dropPower = 0f;
        private bool _isChargingDrop = false;
        private InventoryUIController _inventoryUIController;
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
            IInventoryItem item = PoolManager.Get(itemGlobalID, specificID, enablePhysicsOnDrop);
            droppedItem = item;

            Vector3 finalPos = GetAdjustedDropPosition(_inventoryTarget, pos);
            item.IsItemInInventory = false;

            if (enablePhysicsOnDrop)
            {
                Rigidbody rb = item.GetRigidBody();
                rb.velocity = Vector3.zero;
                rb.position = finalPos; //IMPORTANT LINE, FORCE THE PHYSICS TO UPDATE WHEN OBJECT RE-ENABLED
                                        //EVEN THE MOVE.POSITION DOES NOT WORK
                rb.angularVelocity = Vector3.zero;

                if (finalPos == pos)
                    rb.AddForce(_inventoryTarget.forward * force, ForceMode.Impulse);
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

            // 1. Vérifier si un obstacle est proche avec un OverlapSphere
            Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
            DropOverlappingColliders = colliders;
            if (colliders.Length > lengthToSafePos)
            {
                //Vector3 adjustedPosition = start - target.forward * 0.5f; // Reculer légèrement pour ne pas être dans l'obstacle
                Vector3 adjustedPosition = PlayerHandlerData.MainPlayer.Character.transform.position + Vector3.up; // Reculer légèrement pour ne pas être dans l'obstacle
                Debug.DrawRay(start, -target.forward * 0.5f, Color.yellow, 5.0f);
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
        public void HandleDropInput()
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
                    itemGlobalID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                    specificID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0],
                    pos: _inventoryTarget.position,
                    force: _dropPower,
                    enablePhysicsOnDrop: true,
                    droppedItem: out IInventoryItem droppedItem
                );

                _dropPower = 0f;
                _dropChargeTimer = 0f;
                _isChargingDrop = false;
                AudioHandlerData.AudioManager.PlayRandomEffect(_inventoryTarget.position, Audio.AutoGenerated.EffectsAudioName.Effect_Throw);
                _inventoryUIController.FillDropHoldedImage(0f);
            }
            else if (InputManager.Drop.WasPerformedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                    return;
                DropItem
                (
                    slotID: SlotManager.CurrentSelectedSlot.SlotDatas.SlotID,
                    itemGlobalID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                    specificID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0],
                    pos: _inventoryTarget.position,
                    force: 0.0f,
                    enablePhysicsOnDrop: true,
                    droppedItem: out IInventoryItem droppedItem
                );
                AudioHandlerData.AudioManager.PlayRandomEffect(_inventoryTarget.position, Audio.AutoGenerated.EffectsAudioName.Effect_Throw);
                _inventoryUIController.FillDropHoldedImage(0f);
            }
        }
        #endregion

    }

}
