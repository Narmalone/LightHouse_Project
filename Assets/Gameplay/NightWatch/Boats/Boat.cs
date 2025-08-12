using LightHouse.Game.Sonar.Core;
using System;
using UnityEngine;

namespace LightHouse.Game.Boats
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
        [SerializeField] private VectorPathDatabase _randomPointController;
        [SerializeField] private BoidController _controller;
        [SerializeField] private BoatAnomalyController _anomalyController;

        [Header("Data")]
        [SerializeField] private BoatData _data;

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
        public BoatData Data => _data;

        // ISonarable
        public string Name => gameObject.name;
        public int UniqueID { get; set; }
        public bool IsDetectedBySonar { get; set; }
        public Vector3 Position => _rb.position;
        public Vector3 RotationAngles => _rb.transform.eulerAngles;

        public float RadioFrequency => _defaultRadioFrequency;

        #endregion

        #region Events

        public event Action OnBoatProgressEnded;
        public event Action ForceDotUpdate;

        #endregion

        #region Private Fields

        private float _defaultRadioFrequency;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeBoatData();
            InitializeMovement();
            InitializeAnomalyEvents();
            InitializeRadioFrequency();
        }

        private void LateUpdate()
        {
            HandleMovementProgress();
        }

        private void OnDestroy()
        {
            CleanupBoatData();
            CleanupAnomalyEvents();
        }

        #endregion

        #region Initialization

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
            _defaultRadioFrequency = UnityEngine.Random.Range(157f, 162f);
            _defaultRadioFrequency = (float)Math.Round(_defaultRadioFrequency, 2);
            SonarInfo = $"{_defaultRadioFrequency} MHz";
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
                _anomalyController.TriggerAnomaly(this);
            }
        }

        #endregion

        #region Anomaly Handling

        private void HandleAnomalyResolved()
        {
            DotColor = _aliveDotColor;
            SonarInfo = $"{_defaultRadioFrequency} MHz";
            ForceDotUpdate?.Invoke();
        }

        private void HandleAnomalyAdded()
        {
            DotColor = _deathDotColor;
            //SonarInfo = "156.8 MHz"; // Exemple si besoin de changer aussi la fréquence
            ForceDotUpdate?.Invoke();
        }

        #endregion
    }
}
