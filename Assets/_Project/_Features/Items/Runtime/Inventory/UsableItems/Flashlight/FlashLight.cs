using LightHouse.Core.Inputs;
using UnityEngine.Localization;
using LightHouse.Core.Audio;
using LightHouse.Core.Services;

using System;
using UnityEngine;
using LightHouse.Core.Localization;

namespace LightHouse.Features.Items.Inventory.Flashlight
{
    public class FlashLight : InventoryItemBase, IInventoryItemUsable
    {
        public LocalizedString _lightOn => _inventoryTextsDB.Light_On;
        public LocalizedString _lightOff => _inventoryTextsDB.Light_Off;
        public LocalizedString _holdToAction => _interactionTextsDB.Hold_To_Action;
        [SerializeField] protected string _currentUseInInventoryText;
        [Header("IInventory Item")]
        public bool CanBeUsedFromInventory { get; set; } = true;

        [field: SerializeField] public float UseHoldTime { get; set; } = 1f;
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;

        private bool _isLightOn = false;
        [SerializeField] private Light _light;

        [Header("Audio")]
        [SerializeField] private SO_AudioCue _flashLightOn;
        [SerializeField] private SO_AudioCue _flashLightOff;

        protected override void Awake()
        {
            base.Awake();
            _isLightOn = false;
            _light.gameObject.SetActive(false);
        }

        private void Start()
        {
            UpdateUsableText();
        }

        protected override void LocalizationSettings_SelectedLocaleChanged(Locale obj)
        {
            base.LocalizationSettings_SelectedLocaleChanged(obj);
            UpdateUsableText();
        }

        public async void UpdateUsableText()
        {
            var action = _isLightOn ? _lightOff : _lightOn;
            var key = InputManager.InteractInInventory_Bind_Name;

            _currentUseInInventoryText = await InteractionTextBuilder.Build(
                action,
                key,
                _holdToAction
            );
            InvokeUseTextSlotChanged(_currentUseInInventoryText);
        }


        public void On()
        {
            _isLightOn = true;
            _light.gameObject.SetActive(true);

            if(ServiceLocator.Audio != null && _flashLightOn != null)
                ServiceLocator.Audio.PlayAt(_flashLightOn, this.transform.position);
        }

        public void Off()
        {
            _isLightOn = false;
            _light.gameObject.SetActive(false);
            if (ServiceLocator.Audio != null && _flashLightOn != null)
                ServiceLocator.Audio.PlayAt(_flashLightOff, this.transform.position);
        }

        //IInventoryUSable
        public string UseTextSlot()
        {
            return _currentUseInInventoryText;
        }

        public void UseFromInventory()
        {
            if (_isLightOn)
                Off();
            else
                On();
            OnItemUsed?.Invoke();
            UpdateUsableText();
        }

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }

        public void InvokeUseTextSlotChanged(string newText)
        {
            UseTextSlotChanged?.Invoke(newText);
        }
    }
}

