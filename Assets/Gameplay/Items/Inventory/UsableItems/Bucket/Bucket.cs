using LightHouse.Interactions;
using LightHouse.Items.Interactable;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Bucket : Key, IItemCallback
    {
        [Header(" --- BUCKET --- ")]
        [SerializeField] private IDUseItemTracker _mopTracker;
        public bool IsFilledWithWater = false;

        public void OnRaycastEnd()
        {
            if (IsFilledWithWater)
            {
            
            }
        }

        public void OnRaycastStart()
        {
            if (IsFilledWithWater)
            {
                if (!_mopTracker.gameObject.activeInHierarchy)
                    _mopTracker.gameObject.SetActive(true);
            }
        }

        public override void UseFromInventory()
        {
            base.UseFromInventory();
        }
    }

}
