using LightHouse.Features.Sonar.Core;
using LightHouse.Features.Boats.Frequencies;
using LightHouse.Features.Boats.Anomalies;
using LightHouse.Features.Boats.Nationalities;

using System;
using UnityEngine;

namespace LightHouse.Features.Boats
{
    /// <summary>
    /// Représente un bateau dans le monde.
    /// Gère son mouvement, ses anomalies et ses infos affichées sur le sonar.
    /// </summary>
    public class Boat : MonoBehaviour, ISonarable
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private BoatsNationalitiesManager _boatsManager;
        [SerializeField] private BoatFrequencyAllocator _frequencyAllocator;
        [SerializeField] private VectorPathDatabase _randomPointController;
        [SerializeField] private BoidController _controller;
        [SerializeField] private BoatAnomalyController _anomalyController;

        [Header("Data")]
        [SerializeField] private BoatNationalityDatas _data;

        [Header("Sonar Appearance")]
        [SerializeField] private Color _aliveDotColor;
        [SerializeField] private Color _deathDotColor;
        [field: SerializeField] public Color DotColor { get; set; }
        [field: SerializeField] public Vector2 DotSize { get; set; } = new Vector2(15, 15);
        [field: SerializeField] public Sprite DotSprite { get; set; }
        [field: SerializeField] public string SonarInfo { get; set; }

        #endregion

        #region Properties

        public BoatAnomalyController AnomalyController => _anomalyController;
        public Rigidbody RB => _rb;
        public BoatNationalityDatas Data => _data;

        // ISonarable
        public string Name => gameObject.name;
        public int UniqueID { get; set; }
        public bool IsDetectedBySonar { get; set; }
        public Vector3 Position => _rb.position;
        public Vector3 RotationAngles => _rb.transform.eulerAngles;

        #endregion

        #region Events

        public event Action OnAnomalyPointReached;
        public event Action OnBoatProgressEnded;
        public event Action OnBoatInitialized;
        public Action ForceDotUpdate { get; set; }

        #endregion

        #region Private Fields

        [SerializeField] private float _radioFrequencyMHz;
        public float RadioFrequency => _radioFrequencyMHz;

        #endregion

        #region Unity Lifecycle

        private void LateUpdate()
        {
            HandleMovementProgress();
        }

        private void OnDestroy()
        {
            CleanupBoatData();
            CleanupAnomalyEvents();

            if (_frequencyAllocator != null)
                _frequencyAllocator.ReleaseFrequencyMHz(_radioFrequencyMHz);
        }

        #endregion

        #region Initialization

        public void Initialize()
        {
            InitializeBoatData();
            InitializeMovement();
            InitializeAnomalyEvents();
            InitializeRadioFrequency();
            OnBoatInitialized?.Invoke();
        }

        private void InitializeBoatData()
        {
            DotColor = _aliveDotColor;
            _data = _boatsManager.Register();
            gameObject.name = _data.Name;
            SonarHandlerData.Register(this);
        }

        private void InitializeMovement()
        {
            _controller.Initialize(_randomPointController.GetRandomPath());
        }

        private void InitializeAnomalyEvents()
        {
            _anomalyController.OnAnomalyAdded += HandleAnomalyAdded;
            _anomalyController.OnAnomalyResolved += HandleAnomalyResolved;
        }

        private void InitializeRadioFrequency()
        {
            if (_frequencyAllocator == null)
            {
                Debug.LogError("[Boat] FrequencyAllocator manquant.");
                _radioFrequencyMHz = 157f;
            }
            else
            {
                _radioFrequencyMHz = _frequencyAllocator.AllocateUniqueFrequencyMHz();
            }
            SonarInfo = BoatFrequencyAllocator.FormatFrequencyForDisplay(_radioFrequencyMHz);
        }

        #endregion

        #region Cleanup

        private void CleanupBoatData()
        {
            _boatsManager.Unregister(_data);
            SonarHandlerData.Unregister(this);
        }

        private void CleanupAnomalyEvents()
        {
            _anomalyController.OnAnomalyAdded -= HandleAnomalyAdded;
            _anomalyController.OnAnomalyResolved -= HandleAnomalyResolved;
        }

        #endregion

        #region Movement Logic

        private void HandleMovementProgress()
        {
            if (_controller.Progress >= 1.0f)
            {
                OnBoatProgressEnded?.Invoke();
            }
            else if (!_anomalyController.HasBeenTriggered &&
                     _controller.Progress >= _anomalyController.AnomalyTriggerProgress)
            {
                OnAnomalyPointReached?.Invoke();
                _anomalyController.TriggerAnomaly(this);
            }
        }

        #endregion

        #region Anomaly Handling

        private void HandleAnomalyResolved()
        {
            DotColor = _aliveDotColor;
            ForceDotUpdate?.Invoke();
        }

        private void HandleAnomalyAdded()
        {
            DotColor = _deathDotColor;
            ForceDotUpdate?.Invoke();
        }

        #endregion
    }
}
