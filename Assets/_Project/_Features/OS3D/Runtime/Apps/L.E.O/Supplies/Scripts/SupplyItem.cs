using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>
    /// Entrée d’article du shop : affiche nom, stock, coût, quantité sélectionnée,
    /// et relaie les clics +/- via des évènements.
    /// </summary>
    public class SupplyItem : MonoBehaviour
    {
        #region ===== Serialized References =====

        [SerializeField] private Button _minusButton;
        [SerializeField] private Button _plusButton;
        [SerializeField] private TextMeshProUGUI _itemNameTxt;
        [SerializeField] private TextMeshProUGUI _itemsInStockText;
        [SerializeField] private TextMeshProUGUI _itemCostText;
        [SerializeField] private TextMeshProUGUI _itemAmountText;

        #endregion

        #region ===== Events (relayés vers le contrôleur) =====

        public event Action<SupplyItemDatas> MinusCliqued;
        public event Action<SupplyItemDatas> PlusCliqued;

        #endregion

        #region ===== Runtime State =====

        [field: SerializeField] public SupplyItemDatas Mydatas { get; private set; }

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            // Brancher les boutons une fois
            _minusButton.onClick.AddListener(OnMinusCliqued);
            _plusButton.onClick.AddListener(OnPlusCliqued);

            // Par défaut, rien de sélectionné → on ne peut pas décrémenter
            SetMinusInteractable(false);
        }

        private void OnDestroy()
        {
            _minusButton.onClick.RemoveListener(OnMinusCliqued);
            _plusButton.onClick.RemoveListener(OnPlusCliqued);
        }

        #endregion

        #region ===== Public API =====

        /// <summary>Initialise l’item d’UI avec les données fournies.</summary>
        public void Initialize(SupplyItemDatas datas)
        {
            Mydatas = datas;
            if (Mydatas == null)
            {
                Debug.LogWarning("[SupplyItem] Initialize appelé avec des datas null.");
                return;
            }

            _itemNameTxt.text = Mydatas.Name;
            _itemsInStockText.text = "Coming soon...";
            _itemCostText.text = Mydatas.Cost.ToString("000");

            // Au build du shop, la sélection est en général à 0
            _itemAmountText.text = 0.ToString();

            // Met à jour l’état du bouton - selon la sélection
            UpdateSelectedCountText();
        }

        /// <summary>
        /// Met à jour le libellé quantité et l’état du bouton “-” selon SelectedAmountToBuy.
        /// </summary>
        public void UpdateSelectedCountText()
        {
            if (Mydatas == null) return;

            bool hasSelection = Mydatas.SelectedAmountToBuy > 0;
            SetMinusInteractable(hasSelection);

            _itemAmountText.text = Mydatas.SelectedAmountToBuy.ToString();
        }

        #endregion

        #region ===== Internal: Button Callbacks =====

        private void OnPlusCliqued()
        {
            if (Mydatas == null) return;
            PlusCliqued?.Invoke(Mydatas);
        }

        private void OnMinusCliqued()
        {
            if (Mydatas == null) return;
            MinusCliqued?.Invoke(Mydatas);
        }

        #endregion

        #region ===== Internal: UI Helpers =====

        private void SetMinusInteractable(bool active)
        {
            if (_minusButton != null && _minusButton.interactable != active)
                _minusButton.interactable = active;
        }

        #endregion
    }
}
