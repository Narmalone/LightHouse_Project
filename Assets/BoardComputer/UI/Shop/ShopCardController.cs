using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_itemName;
    [SerializeField] private TextMeshProUGUI m_itemCost;
    [SerializeField] private Button m_buyBtn;

    public ShopItemData ShopItemData;

    public TextMeshProUGUI ItemNameTxt => m_itemName;
    public TextMeshProUGUI ItemCostTxt => m_itemCost;
    public Button BuyBtn => m_buyBtn;

    public void SetCardsInfos(string name, int cost)
    {
        m_itemName.text = name;
        m_itemCost.text = cost.ToString();
    }
}
