using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;
using LightHouse.Features.Interactions;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Features.Items.Interactable
{
    public abstract class InteractableItemBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Item Base")]
        [SerializeField] protected string _name;
        [SerializeField] protected Collider _detectionCollider;

        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public string InteractionText { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action<string> OnNameUpdated;
        protected virtual void Awake()
        {
            
        }

        protected virtual void OnDestroy()
        {
        }

        private void Start()
        {
            InteractionText = GetDefaultInteractionText().Result;
        }

        public virtual string GetName() => this._name;
        public virtual Collider GetCollider() => this._detectionCollider;
        public virtual GameObject GetGameObject() => this.gameObject;

        public abstract void Interact();

        public void InvokeNameUpdated() => OnNameUpdated?.Invoke(_name);
        public void InvokeInteractionDescriptionUpdated() => OnInteractionNameChanged?.Invoke();
        public void InvokeObjectInteracted() => OnObjectInteracted?.Invoke();

        public LocalizedStringDatabase_InteractionTexts _interactionTextsDB;
        public LocalizedString _pressToAction => _interactionTextsDB.Press_To_Action;
        public LocalizedString _interactText => _interactionTextsDB.Use;
        public async virtual Task<string> GetDefaultInteractionText()
        {
            string input = InputManager.Interact_Bind_Name;
            var interactionName = await InteractionTextBuilder.Build(
                _interactText,
                input,
                _pressToAction
            );
            return interactionName;
        }
    }
}

