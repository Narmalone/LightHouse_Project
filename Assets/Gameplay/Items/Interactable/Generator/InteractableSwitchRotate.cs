using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class InteractableSwitchRotate : InteractableSwitch
    {
        #region FIELDS
        [Header("Rotation")]
        [SerializeField] private Transform _targetToRotate;

        [Tooltip("Rotation locale à appliquer lors de l'activation (en degrés)")]
        [SerializeField] private Vector3 _rotationDelta = new Vector3(-90f, 0f, 0f);

        [SerializeField] private float _rotationDuration = 1.0f; // Temps pour compléter la rotation

        [SerializeField] private bool _autoComebackAfterDelay = true;
        [SerializeField] private float _comebackDelay = 60f;
        private Timer _timer;

        private Quaternion _initialRotation;
        private Quaternion _currentTarget;
        private Quaternion _startRotation;

        private bool _isMoving = false;
        private float _rotationTimer = 0f;
        #endregion

        #region UNITY LIFECYCLE
        protected override void Awake()
        {
            base.Awake();
            if (_targetToRotate == null) _targetToRotate = this.transform;

            _initialRotation = _targetToRotate.localRotation;
            _timer = new Timer(_comebackDelay);
            _timer.OnTimerComplete += Timer_OnTimerComplete;
        }

        private void Update()
        {
            if (_isMoving)
            {
                _rotationTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_rotationTimer / _rotationDuration);
                _targetToRotate.localRotation = Quaternion.Slerp(_startRotation, _currentTarget, t);

                if (t >= 1f)
                {
                    _isMoving = false;
                }
            }

            if (_isSwitchOn && _autoComebackAfterDelay)
            {
                _timer.Tick(Time.deltaTime);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _timer.OnTimerComplete -= Timer_OnTimerComplete;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_targetToRotate == null) return;

            Vector3 pos = _targetToRotate.position;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pos, _targetToRotate.right * 0.2f);   // X local
            Gizmos.color = Color.green;
            Gizmos.DrawRay(pos, _targetToRotate.up * 0.2f);      // Y local
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pos, _targetToRotate.forward * 0.2f); // Z local
        }
#endif
        #endregion

        #region INTERACTABLE
        public override void Interact()
        {
            base.Interact();
            if (_isSwitchOn)
                GoToTarget();
            else
                GoToInitial();
        }
        #endregion

        #region TIMER
        private void Timer_OnTimerComplete() => Interact();
        #endregion

        #region ROTATE FUNCS
        public void GoToTarget()
        {
            _startRotation = _targetToRotate.localRotation;
            _currentTarget = Quaternion.Euler(_rotationDelta);
            _rotationTimer = 0f;
            _timer.ResetTimer();
            _timer.StartTimer();
            _isMoving = true;
        }

        public void GoToInitial()
        {
            _startRotation = _targetToRotate.localRotation;
            _currentTarget = _initialRotation;
            _rotationTimer = 0f;
            _isMoving = true;
        }
        #endregion
    }

}
