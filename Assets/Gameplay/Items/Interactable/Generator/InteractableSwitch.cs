using LightHouse.Inputs;
using LightHouse.Interactions;
using LightHouse.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Items.Interactable
{
    public class InteractableSwitch : InteractableItemBase, IInteractable
    {
        #region FIELDS
        [Header(" --- LOCALIZATION --- ")]
        [SerializeField] protected LocalizedStringDatabase_InteractionTexts _interactionsTextsDb;
        [SerializeField] protected InteractionsObjectsType _interactionObjectsType;
        protected LocalizedString _pressToAction => _interactionsTextsDb.Press_To_Action;
        protected LocalizedString _onText;
        protected LocalizedString _offText;
        protected string _currentText;

        protected bool _isSwitchOn = false;
        public bool IsSwitchOn => _isSwitchOn;

        #endregion

        #region MONO CALLBACKS
        protected override void Awake()
        {
            base.Awake();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            InputManager.OnInitialized += InputManager_OnInputManagerInitialized;
            InitializeLocalization();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            InputManager.OnInitialized -= InputManager_OnInputManagerInitialized;
        }
        #endregion

        #region LOCALIZATION

        private void InitializeLocalization()
        {   // Init LocalizedStrings selon le type
            switch (_interactionObjectsType)
            {
                case InteractionsObjectsType.Switch:
                    _onText = _interactionsTextsDb.Enable;
                    _offText = _interactionsTextsDb.Disable;
                    break;

                case InteractionsObjectsType.OpenClose:
                    _onText = _interactionsTextsDb.Open;
                    _offText = _interactionsTextsDb.Close;
                    break;
            }
        }

        protected virtual void OnLocaleChanged(Locale locale)
        {
            UpdateInteractionText();
        }

        /// <summary>
        /// Automatically update the string we want to display under condition
        /// if the switch is on for example, also we can combine with the pressToAction
        /// </summary>
        public async virtual void UpdateInteractionText()
        {
            string input = InputManager.Interact_Bind_Name;
            LocalizedString actionString = _isSwitchOn ? _offText : _onText;

            _currentText = await InteractionTextBuilder.Build(
                actionString,
                input,
                _pressToAction
            );

            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region INPUT MANAGER CALLBACKS
        private void InputManager_OnInputManagerInitialized()
        {
            UpdateInteractionText();
        }
        #endregion

        #region INTERACTIONS METHODS

        public override string GetInteractionName()
        {
            return _currentText ?? "...";
        }

        public override void Interact()
        {
            _isSwitchOn = !_isSwitchOn;
            UpdateInteractionText();
            InvokeObjectInteracted();
        }

        protected override void OnGameInitialized() { }
        #endregion
    }

}
