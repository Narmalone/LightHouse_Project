using MPUIKIT;
using TMPro;
using UnityEngine;

public class ShopCardController : MonoBehaviour
{
    public MPImageBasic ItemBackground;
    public MPImageBasic ItemIcon;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
    public TextMeshProUGUI ItemCost;
    public TextMeshProUGUI dollarText;

    public ShopItemData ItemDataInstance;
    private ShopItemData _itemData;

    private void Awake()
    {
        ItemDataInstance = ScriptableObject.CreateInstance<ShopItemData>();
    }

    public void SetItemData(ShopItemData item)
    {
        _itemData = item;
        ItemDataInstance.Name = _itemData.Name;
        ItemDataInstance.Description = _itemData.Description;
        ItemDataInstance.BaseCost = _itemData.BaseCost;
        ItemDataInstance.StockItems = _itemData.StockItems;
        ItemDataInstance.Prefab = _itemData.Prefab;

        UpdateCardInfoFromItemData();
    }

    public virtual void UpdateCardInfoFromItemData()
    {
        ItemName.text = ItemDataInstance.Name;
        ItemDescription.text = ItemDataInstance.Description;
        ItemCost.text = ItemDataInstance.BaseCost.ToString();
    }
}
