using LightHouse.Interactions;
using LightHouse.Localization;
using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Items
{
    public class SingleRaycastedItemName : MonoBehaviour, IItemName
    {
        #region FIELDS & PROPERTIES
#pragma warning disable
        public event Action<string> OnNameUpdated;

        [SerializeField] protected string _itemName;
        [SerializeField] private Collider _col;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        [Header(" --- LOCALIZATION --- ")]
        [SerializeField] protected LocalizedStringDatabase_InteractionTexts _interactionTexts;
        [SerializeField] protected LocalizedString[] _keys;
        protected LocalizedString _currentLocalizedString;
        #endregion

        #region MONO CALLBACKS

        private void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
            UpdateInteractionText();
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
        }
        #endregion

        #region IItemName
        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public string GetName()
        {
            return _itemName;
        }
        #endregion

        #region LOCALIZATION

        private void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            UpdateInteractionText();
        }

        protected virtual async void UpdateInteractionText()
        {
            

            // Résolution de toutes les chaînes de manière asynchrone
            string[] resolvedParts = new string[_keys.Length];
            for (int i = 0; i < _keys.Length; i++)
            {
                var op = _keys[i].GetLocalizedStringAsync();
                await op.Task;
                resolvedParts[i] = op.Result;
            }

            // Jointure des textes avec un espace
            _itemName = string.Join(" ", resolvedParts).Trim();

            if (IsItemRaycasted)
                OnNameUpdated?.Invoke(_itemName);
        }
        #endregion

    }

}
