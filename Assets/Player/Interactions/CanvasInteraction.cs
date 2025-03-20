using TMPro;
using UnityEngine;

namespace LightHouse.Interactions
{
    public class CanvasInteraction : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemName_TMP;
        [SerializeField] private TextMeshProUGUI _itemDescription_TMP;
        [SerializeField] private TextMeshProUGUI _itemInventory_TMP;
        [SerializeField] private CanvasGroup _group;

        public TextMeshProUGUI ItemName_TMP => _itemName_TMP;
        public TextMeshProUGUI ItemDescription_TMP => _itemDescription_TMP;
        public TextMeshProUGUI ItemInventory_TMP => _itemInventory_TMP;

        private bool _isHided = false;
        public bool IsHided => _isHided;    

        public void Hide()
        {
            _group.alpha = 0.0f;
            _isHided = true;
        }

        public void Show()
        {
            _group.alpha = 1.0f;
            _isHided = false;
        }

        public void HideItemName()
        {
            _itemName_TMP.gameObject.SetActive(false);
        }

        public void ShowItemName()
        {
            _itemName_TMP.gameObject.SetActive(true);
        }

        public void HideItemInteractionName()
        {
            _itemDescription_TMP.gameObject.SetActive(false);
        }

        public void ShowItemInteractionName()
        {
            _itemDescription_TMP.gameObject.SetActive(true);
        }

        public void HideItemPickup()
        {
            _itemInventory_TMP.gameObject.SetActive(false);
        }

        public void ShowItemPickup()
        {
            _itemInventory_TMP.gameObject.SetActive(true);
        }
    }

}
