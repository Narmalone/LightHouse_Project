using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LightHouse.Features.Items.Interactable
{
    public class InteractableSwitch : InteractableItemBase
    {
        #region FIELDS
        [Header(" --- LOCALIZATION --- ")]
        [SerializeField] protected LocalizedStringDatabase_InteractionTexts _interactionsTextsDb;
        [SerializeField] protected InteractionsObjectsType _interactionObjectsType;
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
            InitializeLocalization();
        }

        private void Start()
        {
            //UpdateInteractionText();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
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
            //UpdateInteractionText();
        }

        /// <summary>
        /// Automatically update the string we want to display under condition
        /// if the switch is on for example, also we can combine with the pressToAction
        /// </summary>
        public async virtual void UpdateInteractionText()
        {
            string input = InputManager.Interact_Bind_Name;
            LocalizedString actionString = _isSwitchOn ? _offText : _onText;

            InteractionText = await InteractionTextBuilder.Build_Hold_To_Action(
                actionString,
                input,
                _pressToAction
            );

            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region INTERACTIONS METHODS

        public override void Interact()
        {
            _isSwitchOn = !_isSwitchOn;
            //UpdateInteractionText();
            InvokeObjectInteracted();
        }

        #endregion
    }

}
