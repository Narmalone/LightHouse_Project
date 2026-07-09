using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;
using LightHouse.Features.Interactions;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LightHouse.Features.Items.Interactable
{
    public abstract class   InteractableItemBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Item Base")]
        [SerializeField] protected string _name;
        [SerializeField] protected LocalizedString _interactableItemName;
        [SerializeField] protected Collider _detectionCollider;

        public LocalizedStringDatabase_InteractionTexts _interactionTextsDB;
        public LocalizedString _pressToAction => _interactionTextsDB.Press_To_Action;
        public LocalizedString _interactText => _interactionTextsDB.Use;

        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public string InteractionText { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action<string> OnNameUpdated;

        protected virtual void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
        }


        private async void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            await GetDefaultNameText();
            await GetDefaultInteractionText();
        }

        protected virtual void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;
        }

        protected virtual async void Start()
        {
            InteractionText = GetDefaultInteractionText().Result;
            await GetDefaultNameText();
        }

        public virtual string GetName() => this._name;
        public virtual Collider GetCollider() => this._detectionCollider;
        public virtual GameObject GetGameObject() => this.gameObject;

        public abstract void Interact();

        public void InvokeNameUpdated() => OnNameUpdated?.Invoke(_name);
        public void InvokeInteractionDescriptionUpdated() => OnInteractionNameChanged?.Invoke();
        public void InvokeObjectInteracted() => OnObjectInteracted?.Invoke();

        public async virtual Task<string> GetDefaultInteractionText()
        {
            string input = InputManager.Interact_Bind_Name;
            var interactionName = await InteractionTextBuilder.Build_Hold_To_Action(
                _interactText,
                input,
                _pressToAction
            );
            return interactionName;
        }

        public async virtual Task<string> GetDefaultNameText()
        {
            if (_interactableItemName == null ||_interactableItemName.TableReference == null) return string.Empty;
            AsyncOperationHandle<string> actionTextOp = _interactableItemName.GetLocalizedStringAsync();
            await actionTextOp.Task;
            _name = actionTextOp.Result;
            _interactableItemName.RefreshString();
            return _name;
        }

         
    }
}

