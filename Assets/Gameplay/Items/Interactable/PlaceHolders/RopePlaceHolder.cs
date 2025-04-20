using UnityEngine;

public class RopePlaceHolder : IDUseItemTracker
{
    [SerializeField] private GameObject[] _objectsToEnable;
    [SerializeField] private GameObject[] _objectsToDisable;
    protected override void Usable_OnItemUsed()
    {
        base.Usable_OnItemUsed();
        foreach(GameObject obj in _objectsToEnable)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in _objectsToDisable)
        {
            obj.SetActive(false);
        }
    }
}
