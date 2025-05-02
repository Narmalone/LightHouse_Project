using System.Collections.Generic;
using LightHouse.Items.Interactable;
using LightHouse.Items.Inventory;
using UnityEngine;

namespace LightHouse.Game.Tutorial
{
    public class TutorialSwitchboard : MonoBehaviour
    {
        [SerializeField] private Key _shedKey;
        [SerializeField] private List<BernacleInteractable> _objetsToRemoveBeforeInteract;
        [SerializeField] private ElectricalPannel _electricalPannel;

        private void Awake()
        {
            _electricalPannel.ElectricDoor.CanBeInteracted = false;
            _electricalPannel.ElectricDoor.CanBeRaycasted = false;
            _electricalPannel.EnableElectricityFirstCollider.GetCollider().enabled = false;

            _shedKey.GetCollider().enabled = false;

            foreach (var obj in _objetsToRemoveBeforeInteract)
            {
                obj.OnDestroyed += () =>
                {
                    BernacleInteractable removedBernacle = _objetsToRemoveBeforeInteract.Find(x => x.transform.position == obj.transform.position);
                    _objetsToRemoveBeforeInteract.Remove(removedBernacle);
                    if (_objetsToRemoveBeforeInteract.Count <= 0)
                        OnBernaclesRemoved();

                };
            }
        }

        private void OnBernaclesRemoved()
        {
            _electricalPannel.EnableElectricityFirstCollider.GetCollider().enabled = true;
            _shedKey.GetCollider().enabled = true;

            _electricalPannel.ElectricDoor.CanBeInteracted = true;
            _electricalPannel.ElectricDoor.CanBeRaycasted = true;
        }
    }

}
