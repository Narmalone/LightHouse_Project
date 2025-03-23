using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class Generator : MonoBehaviour
    {
        [Header("Fuel Settings")]
        [SerializeField] private float _fuelLevel = 0f;
        [SerializeField] private float _fuelDecrementationSpeed = 1.0f;

        [Header("DEBUG")]
        [SerializeField] private bool _isGeneratorOn;
        [SerializeField] private GeneratorSwitch _generatorSwitch;
        [SerializeField] private SingleItemName _fuelIsRequired;
        [SerializeField] private JerricanReceiver _jerricanReceiver;

        private JerricanEssence _lastJerricanSelected;
        private PlayerInventory _playerInventory;

        public event Action OnFuelDown;
        public event Action<bool> OnGeneratorSwitchChanged;

        #region PROPERTIES
        public bool IsGeneratorOn
        {
            get => _isGeneratorOn;
            private set
            {
                _isGeneratorOn = value;
                OnGeneratorSwitchChanged?.Invoke(value);
            }
        }
        #endregion

        #region MONO'S Callbacks

        private void Awake()
        {
            RegisterCallbacks();
            Initialize();
        }

        private void Start()
        {
            _jerricanReceiver.ItemDescription = "You need a jerrican to fill the generator";
        }

        private void Update()
        {
            if (_isGeneratorOn)
            {
                _fuelLevel -= Time.deltaTime * _fuelDecrementationSpeed;

                if (_fuelLevel <= 0f)
                {
                    OnGeneratorFuelDown();
                    OnFuelDown?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }

        #endregion

        #region INITIALIZE

        private void Initialize()
        {
            _fuelIsRequired.gameObject.SetActive(_fuelLevel > 0 ? false : true);
        }

        #endregion

        #region REGISTERING CALLBACKS
        private void RegisterCallbacks()
        {
            _generatorSwitch.OnObjectInteracted += GeneratorSwitch_OnObjectInteracted;
            _jerricanReceiver.OnObjectInteracted += JerricanReceiver_OnInteracted;
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnItemHandChanged;
        }

        private void UnregisterCallbacks()
        {
            _generatorSwitch.OnObjectInteracted -= GeneratorSwitch_OnObjectInteracted;
            _jerricanReceiver.OnObjectInteracted -= JerricanReceiver_OnInteracted;
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnItemHandChanged;
        }

        #endregion

        #region INVENTORY CALLBACKS

        private void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _playerInventory = obj;
        }

        private void PlayerInventory_OnItemHandChanged(IInventoryItem obj)
        {
            if (obj is JerricanEssence)
            {
                _jerricanReceiver.ItemDescription = $"Press {InputManager.GetBindingName(InputManager.Interact)} to use jerrican";
                _jerricanReceiver.CanBeInteracted = true;
                _lastJerricanSelected = obj as JerricanEssence;
            }
            else
            {
                _jerricanReceiver.ItemDescription = "You need a jerrican to fill the generator";
                _jerricanReceiver.CanBeInteracted = false;
                _lastJerricanSelected = null;
            }

            if(_jerricanReceiver.IsItemRaycasted) _jerricanReceiver.InvokeInteractionNameChanged();
        }
        #endregion

        #region CONTROLLERS CALLBACKS
        private void JerricanReceiver_OnInteracted()
        {
            if (_lastJerricanSelected != null)
            {
                AddFuel(_lastJerricanSelected.EssenceAmount);
                _jerricanReceiver.InvokeInteractionNameChanged();
                JerricanEssence jerrican = _lastJerricanSelected.InvokeRemoveItemInInventory();
                Destroy(jerrican.gameObject);
            }
        }

        private void GeneratorSwitch_OnObjectInteracted()
        {
            if(!_isGeneratorOn && _fuelLevel > 0f && _generatorSwitch.IsSwitchOn)
            {
                StartGenerator();
            }
            else if(_isGeneratorOn && !_generatorSwitch.IsSwitchOn)
            {
                StopGenerator();
            }
        }
        #endregion

        #region GENERATOR FUNCTIONS

        public void StartGenerator()
        {
            IsGeneratorOn = true;
        }

        public void StopGenerator()
        {
            IsGeneratorOn = false;
        }

        private void OnGeneratorFuelDown()
        {
            _fuelLevel = 0f;
            StopGenerator();
            _fuelIsRequired.gameObject.SetActive(true);
        }

        public void SetFuel(float fuelValue)
        {
            _fuelLevel = fuelValue;
            OnFuelChanged();
        }

        public void AddFuel(float fuelValue) 
        {
            _fuelLevel += fuelValue;
            OnFuelChanged();
        }

        public void OnFuelChanged()
        {
            if(_fuelLevel > 0f && _fuelIsRequired.isActiveAndEnabled)
            {
                _fuelIsRequired.gameObject.SetActive(false);
            }
            else if(_fuelLevel <= 0f && !_fuelIsRequired.isActiveAndEnabled)
            {
                _fuelIsRequired.gameObject.SetActive(true);
            }
        }

        #endregion
    }

}
