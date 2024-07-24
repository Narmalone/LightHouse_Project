using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private byte slots = 4;
    [SerializeField] private byte currentUsedSlots = 0;
    public List<GameObject> objectsInInventory = new List<GameObject>();
    public List<Image> buttonsInInventory = new List<Image>();

    public void AddItemToInventory(GameObject obj)
    {
        if(currentUsedSlots >= slots)
        {
            Debug.Log("Slots max utilisées");
            return;
        }
        buttonsInInventory[currentUsedSlots].color = Color.gray;
        currentUsedSlots++;
        objectsInInventory.Add(obj);
    }

    public void RemoveItemFromInventory(GameObject obj)
    {
        if(objectsInInventory.Contains(obj))
        {
            buttonsInInventory[currentUsedSlots].color = Color.white;
            currentUsedSlots--;
            objectsInInventory.Remove(obj);
        }
    }

    public void RemoveItemFromInventoryAtIndex(int index)
    {
        buttonsInInventory[index].color = Color.white;
        currentUsedSlots--;
        objectsInInventory.RemoveAt(index);
    }
}
