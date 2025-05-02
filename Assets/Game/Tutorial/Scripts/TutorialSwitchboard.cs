using System.Collections.Generic;
using LightHouse.Items.Interactable;
using LightHouse.Items.Inventory;
using UnityEngine;

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
                var re = _objetsToRemoveBeforeInteract.Find(x => x.transform.position == obj.transform.position);
                _objetsToRemoveBeforeInteract.Remove(obj);
                if(_objetsToRemoveBeforeInteract.Count <= 0)
                {
                    OnBernaclesRemoved();
                }
                Debug.Log("he has been destroyed");
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
