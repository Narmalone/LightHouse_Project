using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotController : MonoBehaviour, IPointerClickHandler
{
    public RectTransform RectTransform;
    public ItemBase Item;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(eventData.clickCount);
        if(Item != null && eventData.clickCount >= 2)
        {
            Debug.Log("Item pas nul & double clique");
            //Voir aussi s'il y'a de la place dans l'inventaire
            if (PlayerManager.Instance._inventory.IsInventoryFull)
            {
                Debug.Log("INVENTAIRE FULL");
            }
            else
            {
                Debug.Log("Inventaire pas full");
            }
        }
    }
}
