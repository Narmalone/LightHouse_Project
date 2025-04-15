using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items
{
    public class Generator : MonoBehaviour
    {
        [Header("Fuel Settings")]
        [SerializeField] private float _fuelLevel = 0f;
        [SerializeField] private float _fuelDecrementationSpeed = 1.0f;

        [Header("DEBUG")]
        [SerializeField] private bool _isGeneratorOn;
        [SerializeField] private InteractableSwitch _generatorSwitch;
        [SerializeField] private SingleItemName _fuelIsRequired;
        [SerializeField] private JerricanItemTracker _jerricanReceiver;

        private JerricanEssence _lastJerricanSelected;

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
            _jerricanReceiver.OnJerricanUsed += JerricanReceiver_OnJerricanUsed;
        }

        private void UnregisterCallbacks()
        {
            _generatorSwitch.OnObjectInteracted -= GeneratorSwitch_OnObjectInteracted;
            _jerricanReceiver.OnJerricanUsed -= JerricanReceiver_OnJerricanUsed;
        }

        #endregion

        #region CONTROLLERS CALLBACKS
        private void JerricanReceiver_OnJerricanUsed(float essenceAmount)
        {
            AddFuel(essenceAmount);
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
