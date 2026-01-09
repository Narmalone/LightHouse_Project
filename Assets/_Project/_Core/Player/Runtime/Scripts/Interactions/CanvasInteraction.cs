using TMPro;
using UnityEngine;

namespace LightHouse.Interactions
{
    public class CanvasInteraction : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemName_TMP;
        [SerializeField] private TextMeshProUGUI _itemInteractionName_TPM;
        [SerializeField] private TextMeshProUGUI _itemPickup_TPM;
        [SerializeField] private CanvasGroup _group;

        public TextMeshProUGUI ItemName_TMP => _itemName_TMP;
        public TextMeshProUGUI ItemInteractionName_TMP => _itemInteractionName_TPM;
        public TextMeshProUGUI ItemPickup_TPM => _itemPickup_TPM;

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
            _itemInteractionName_TPM.gameObject.SetActive(false);
        }

        public void ShowItemInteractionName()
        {
            _itemInteractionName_TPM.gameObject.SetActive(true);
        }

        public void HideItemPickup()
        {
            _itemPickup_TPM.gameObject.SetActive(false);
        }

        public void ShowItemPickup()
        {
            _itemPickup_TPM.gameObject.SetActive(true);
        }

        public void SetItemPickupText(string text)
        {
            _itemPickup_TPM.text = text;
        }
    }

}
