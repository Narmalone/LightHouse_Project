using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopCardController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_itemName;
    [SerializeField] private TextMeshProUGUI m_itemCost;

    public TextMeshProUGUI ItemNameTxt => m_itemName;
    public TextMeshProUGUI ItemCostTxt => m_itemCost;

    public void SetCardsInfos(string name, int cost)
    {
        m_itemName.text = name;
        m_itemCost.text = cost.ToString();
    }
}
