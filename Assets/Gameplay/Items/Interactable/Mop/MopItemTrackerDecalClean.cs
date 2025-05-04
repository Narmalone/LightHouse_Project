using LightHouse.Handlers;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Items.Interactable
{
    public class MopItemTrackerDecalClean : IDUseItemTracker
    {
        [Header("DECALS")]
        [SerializeField] private DecalProjector _targetDecal;

        protected override void OnGameInitialized()
        {
            base.OnGameInitialized();
            PlayerHandlerData.MainPlayer.Character.IgnoreCollider(this._detectionCollider);
        }

        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            this.gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            if(_targetDecal == null)
                _targetDecal = GetComponent<DecalProjector>();
        }
    }

}
