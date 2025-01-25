using UnityEngine;

public class Food : ItemBase
{
    [SerializeField] private CustomEvent_Float _eventPlayerEat;
    [SerializeField] private float _foodAmount;

    [SerializeField] private string _foodName;
    public override string Name { get => _foodName; set => _foodName = value; }
    public override bool Use()
    {
        base.Use();
        isUsable = false;
        isInventoryItem = false;

        _eventPlayerEat.Raise(_foodAmount);
        Destroy(gameObject);
        return true;
    }
}