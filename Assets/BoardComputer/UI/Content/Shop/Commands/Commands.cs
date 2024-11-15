using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commands : MonoBehaviour
{
    public Transform InstParent;
    public ScrollWindowController Scroll;
    public void MoveCartToCommands(List<ShopCartItem> items)
    {
        foreach(ShopCartItem itm in items)
        {
            itm.transform.parent = InstParent;
            itm.RemoveButtonParent.gameObject.SetActive(false);
        }
        Scroll.UpdateContentTransform();
    }
}
