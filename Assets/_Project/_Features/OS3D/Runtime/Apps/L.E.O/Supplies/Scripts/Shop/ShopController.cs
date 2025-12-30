using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace LightHouse.Game.Computer.LEO.Supplies
{
    /// <summary>
    /// Contrôle l’affichage du shop des fournitures :
    /// - Construit la liste d’items par catégorie à partir d’une config runtime
    /// - Gère le switch de catégories (onglets)
    /// - Relaye les clics (+/-) des items via des évènements vers le manager
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        #region ===== Serialized References (assignées dans l’Inspector) =====

        [Header("Prefabs & UI Parents")]
        [SerializeField] private SupplyItem _supplyItemPrefab;

        [Tooltip("Conteneurs par catégorie (clé = catégorie, valeur = parent RectTransform)")]
        [SerializeField] private SerializedDictionary<E_SupplyCategory, RectTransform> _categoryParents;

        [Header("Category Buttons")]
        [SerializeField] private SupplyCategoryButton[] _shopCategoryButtons;

        #endregion

        #region ===== Runtime State =====

        // id -> item instancié côté shop (pour refresh rapides)
        private readonly Dictionary<int, SupplyItem> _shopItems = new();

        private E_SupplyCategory _currentCategory;
        private RectTransform _currentParent;

        public E_SupplyCategory CurrentCategory => _currentCategory;

        #endregion

        #region ===== Events (émis vers le manager) =====

        public event Action<SupplyItemDatas> OnShopPlus;
        public event Action<SupplyItemDatas> OnShopMinus;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            RegisterToButtons();
        }

        private void OnDestroy()
        {
            UnregisterToButtons();
            Clear();
        }

        #endregion

        #region ===== Public API =====

        /// <summary>
        /// Construit le shop : instancie tous les items dans leurs parents de catégorie,
        /// branche les callbacks, et masque les catégories par défaut (le manager appelle SwitchTo ensuite).
        /// </summary>
        public void BuildShop(Dictionary<E_SupplyCategory, SupplyItemDatas[]> runtimeConfig)
        {
            Clear();

            if (runtimeConfig == null || runtimeConfig.Count == 0)
            {
                Debug.LogWarning("[ShopController] BuildShop: runtimeConfig est vide.");
                return;
            }

            foreach (var kvp in runtimeConfig)
            {
                var category = kvp.Key;
                var items = kvp.Value;

                if (!_categoryParents.TryGetValue(category, out var parent) || parent == null)
                {
                    Debug.LogWarning($"[ShopController] Aucun parent trouvé pour la catégorie {category}.");
                    continue;
                }

                if (items == null || items.Length == 0)
                {
                    // On masque quand même la catégorie si aucun item
                    SetParentActive(parent, false);
                    continue;
                }

                foreach (var data in items)
                {
                    if (data == null) continue;
                    CreateAndRegisterShopItem(parent, data);
                }

                // Masquer la catégorie; le manager décidera laquelle afficher
                SetParentActive(parent, false);
            }
        }

        /// <summary>
        /// Active l’onglet / parent d’une catégorie et désactive l’ancien.
        /// </summary>
        public void SwitchTo(E_SupplyCategory targetCategory)
        {
            if (!_categoryParents.TryGetValue(targetCategory, out var parent) || parent == null)
            {
                Debug.LogWarning($"[ShopController] SwitchTo: parent introuvable pour {targetCategory}.");
                return;
            }

            // Disable ancien parent si besoin
            if (_currentParent != null && _currentParent.gameObject.activeInHierarchy)
                SetParentActive(_currentParent, false);

            _currentParent = parent;
            _currentCategory = targetCategory;
            SetParentActive(parent, true);
        }

        /// <summary>
        /// Met à jour l’affichage des compteurs pour un item donné.
        /// </summary>
        public void RefreshShopItem(SupplyItemDatas datas)
        {
            if (datas == null) return;

            if (_shopItems.TryGetValue(datas.UniqueID, out var item) && item != null)
                item.UpdateSelectedCountText();
            //Debug.Log(item.Mydatas.Prefab.name);
        }

        /// <summary>
        /// Met à jour l’affichage des compteurs pour tous les items.
        /// </summary>
        public void RefreshAllShopItem()
        {
            foreach (var item in _shopItems.Values)
            {
                if (item != null)
                    item.UpdateSelectedCountText();
            }
        }

        /// <summary>
        /// Détruit tous les items instanciés et vide le cache.
        /// </summary>
        public void Clear()
        {
            foreach (var kvp in _shopItems)
            {
                var item = kvp.Value;
                if (item == null) continue;

                item.PlusCliqued -= HandlePlus;
                item.MinusCliqued -= HandleMinus;
                Destroy(item.gameObject);
            }
            _shopItems.Clear();
        }

        #endregion

        #region ===== Internal: Button wiring =====

        private void RegisterToButtons()
        {
            if (_shopCategoryButtons == null) return;

            foreach (var button in _shopCategoryButtons)
            {
                if (button == null) continue;
                button.OnSupplyCategoryClicked += Button_OnSupplyCategoryClicked;
            }
        }

        private void UnregisterToButtons()
        {
            if (_shopCategoryButtons == null) return;

            foreach (var button in _shopCategoryButtons)
            {
                if (button == null) continue;
                button.OnSupplyCategoryClicked -= Button_OnSupplyCategoryClicked;
            }
        }

        private void Button_OnSupplyCategoryClicked(E_SupplyCategory category)
        {
            SwitchTo(category);
        }

        #endregion

        #region ===== Internal: Category / Item helpers =====

        /// <summary>
        /// Instancie un SupplyItem, l’initialise, branche ses événements et le met en cache.
        /// </summary>
        private void CreateAndRegisterShopItem(RectTransform parent, SupplyItemDatas data)
        {
            var item = Instantiate(_supplyItemPrefab, parent);
            item.name = data.Name;
            item.Initialize(data);

            // Route boutons vers events publics
            item.PlusCliqued += HandlePlus;
            item.MinusCliqued += HandleMinus;

            if (_shopItems.ContainsKey(data.UniqueID))
                Debug.LogWarning($"[ShopController] Duplicate UniqueID détecté: {data.UniqueID} ({data.Name}). Dernier item référencé écrasera l’ancien.");

            _shopItems[data.UniqueID] = item;
        }

        /// <summary>
        /// Active/désactive proprement un parent de catégorie.
        /// </summary>
        private static void SetParentActive(RectTransform parent, bool active)
        {
            if (parent != null && parent.gameObject.activeSelf != active)
                parent.gameObject.SetActive(active);
        }

        #endregion

        #region ===== Internal: Event routing to manager =====

        private void HandlePlus(SupplyItemDatas d) => OnShopPlus?.Invoke(d);
        private void HandleMinus(SupplyItemDatas d) => OnShopMinus?.Invoke(d);

        #endregion
    }
}
