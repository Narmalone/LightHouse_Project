using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
    public int TotalPriceInCart;
    public Button CommandButton;
    public ScrollWindowController Scroller;
    public List<ShopCartItem> Controllers = new List<ShopCartItem>();
    public ShopCartItem Prefab;
    public RectTransform InstParent;
    [SerializeField] private TextMeshProUGUI _cartPricesTxt;
    public event Action<ShopCartItem> OnRemoveItemCliqued;
    
    public ShopCartItem OnItemAddedToCart(int itemPrice, ShopItemData itemData, ShopItemState boughtFromItemState, Action<ShopCartItem> removedItemTarget)
    {
        TotalPriceInCart += itemPrice;
        ShopCartItem controller = Instantiate(Prefab, InstParent);
        controller.StoredPrice = itemPrice;
        controller.SetItemData(itemData);
        controller.BoughtFromItemState = boughtFromItemState;
        
        controller.RemoveFromCartButton.onClick.AddListener(() =>
        {
            removedItemTarget?.Invoke(controller);
            OnRemoveItemCliqued?.Invoke(controller);
        });
        //check si le joueur à assez d'argent pour passer la commande
        _cartPricesTxt.text = TotalPriceInCart.ToString();
        Scroller.UpdateContentTransform();
        Controllers.Add(controller);
        return controller;
    }

    public void ResetCart(bool destroyObjects = false, bool resetPrice = true)
    {
        if (destroyObjects)
        {
            for(int i = 0; i < Controllers.Count; i++)
            {
                Destroy(Controllers[i].gameObject);
            }
        }
        Controllers.Clear();
        if (resetPrice)
            ResetCartPrice();
    }

    public void SetCartPriceText(int totalCart)
    {
        _cartPricesTxt.text = totalCart.ToString();
    }

    public void UpdateCartPrice()
    {
        _cartPricesTxt.text = TotalPriceInCart.ToString();
    }

    public void ResetCartPrice()
    {
        _cartPricesTxt.text = "0";
        TotalPriceInCart = 0;
    }
}
