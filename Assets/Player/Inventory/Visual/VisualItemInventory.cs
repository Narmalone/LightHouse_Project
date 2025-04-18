using LightHouse.Handlers;
using UnityEngine;

namespace LightHouse.Inventory
{
    public class VisualItemInventory : MonoBehaviour
    {
        private IInventoryItem _currentHandedItem;

        #region MONOS CALLBACK
        private void Awake()
        {
            InventoryHandlerData.OnSelectedItemChanged += InventoryHandlerData_OnSelectedItemChanged;
            SlotManager.OnSlotSelectedChanged += SlotManager_OnSlotSelectedChanged;
            InventoryHandlerData.OnItemDropped += InventoryHandlerData_OnItemDropped;
            InventoryHandlerData.OnItemAddedToInventory += InventoryHandlerData_OnItemAddedToInventory;
        }

        private void OnDestroy()
        {
            InventoryHandlerData.OnSelectedItemChanged -= InventoryHandlerData_OnSelectedItemChanged;
            InventoryHandlerData.OnItemDropped -= InventoryHandlerData_OnItemDropped;
            SlotManager.OnSlotSelectedChanged -= SlotManager_OnSlotSelectedChanged;
            InventoryHandlerData.OnItemAddedToInventory -= InventoryHandlerData_OnItemAddedToInventory;
        }

        private void LateUpdate()
        {
            if (_currentHandedItem != null)
            {
                //set this item position & rotation just in front of the player
                Vector3 targetPos = PlayerHandlerData.MainPlayer.Character.transform.position;
                transform.position = new Vector3(targetPos.x, targetPos.y + (PlayerHandlerData.MainPlayer.Character.CurrentHeight - 0.75f), targetPos.z);
                transform.rotation = PlayerHandlerData.MainPlayer.PlayerCamera.transform.rotation;

                Transform currentItmTransform = _currentHandedItem.GetGameObject().transform;
                //Apply global position
                currentItmTransform.position = transform.position;
                currentItmTransform.rotation = transform.rotation;

                //Apply offsets
                currentItmTransform.localPosition = new Vector3(0.0f, 0.0f, 0.75f) + _currentHandedItem.InventoryLocalPositionOffset;
                currentItmTransform.localRotation = Quaternion.Euler(_currentHandedItem.InventoryEulerAnglesForLocalRotation);
            }
        }
        #endregion

        #region InventoryHandler Callbacks
        private void InventoryHandlerData_OnItemAddedToInventory(IInventoryItem obj)
        {
            if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex)) return;
            //Si on n'a pas à ce moment pas d'item visuel
            if (obj != null && _currentHandedItem == null)
            {
                _currentHandedItem = obj;
                obj.GetGameObject().SetActive(true);
                _currentHandedItem.GetGameObject().transform.SetParent(this.transform);
            }
        }
        private void InventoryHandlerData_OnSelectedItemChanged(IInventoryItem obj)
        {
            //Si on change d'item sélectionné et que c'est un autre item, on désactive l'actuel qui va être l'ancien
            if (obj != null && _currentHandedItem != null)
            {
                _currentHandedItem.GetGameObject().SetActive(false);
                _currentHandedItem = null;
                return;
            }
        }

        private void InventoryHandlerData_OnItemDropped(IInventoryItem obj)
        {
            //on reset l'item pour ne plus mettre à jour pos + rotation
            _currentHandedItem = null;
            if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || !SlotManager.CurrentSelectedSlot.SlotDatas.HasItem) return;

            //On regarde si il y'a un autre item dans dans notre slot
            if (SlotManager.CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem itm))
            {
                _currentHandedItem = itm;
                itm.GetGameObject().SetActive(true);
                itm.GetRigidBody().isKinematic = true;
                _currentHandedItem.GetGameObject().transform.SetParent(this.transform);
                return;
            }
        }

        #endregion

        #region SlotManager Callback
        private void SlotManager_OnSlotSelectedChanged()
        {
            if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || !SlotManager.CurrentSelectedSlot.SlotDatas.HasItem)
            {
                if (_currentHandedItem != null)
                {
                    _currentHandedItem.GetGameObject().SetActive(false);
                    _currentHandedItem = null;
                }
                return;
            }

            //Si on a un item mais qu'on a scroll / select
            if (SlotManager.CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem itm))
            {
                _currentHandedItem = itm;
                itm.GetGameObject().SetActive(true);
                itm.GetRigidBody().isKinematic = true;
                _currentHandedItem.GetGameObject().transform.SetParent(this.transform);
                return;
            }
        }
        #endregion
    }

}
