using LightHouse.Core.Inputs;
using UnityEngine;
using UnityEngine.Localization;
using LightHouse.Core.Localization;
using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using LightHouse.Core.Player;

namespace LightHouse.Features.Items.Interactable.Doors
{
    public class LockedDoor : IDUseItemTracker, IDoor
    {
        #region SERIALIZED FIELDS
        [Header(" --- LOCKED DOOR --- ")]
        [SerializeField] protected Transform _pivot; // Le pivot de la porte
        [SerializeField] protected Vector3 _openRotationAngles = new Vector3(0, 90f, 0f);
        protected float _rotationDuration = 2f; // Durée en secondes (écrasée si on récupère la durée depuis l'audio)
        protected float _rotationStartTime;

        [Header(" --- LOCALIZATION --- ")]
        protected LocalizedString _openText => _interactionTextsDB.Open;
        protected LocalizedString _closeText => _interactionTextsDB.Close;
        protected LocalizedString _unlockText => _interactionTextsDB.Unlock;

        [Header(" --- ANIMATION --- ")]
        [SerializeField] protected AnimationCurve _openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected AnimationCurve _closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header(" --- SOUND EFFECTS --- ")]
        [SerializeField] private SO_AudioCue _openDoor;
        [SerializeField] private SO_AudioCue _closeDoor;
        [SerializeField] private SO_AudioCue _lockedDoor;
        [SerializeField] private SO_AudioCue _unlockDoor;

        [Header("Debug")]
        [SerializeField] protected bool _isUnLocked = false;

        public bool IsUnLocked => _isUnLocked;

        protected bool _isOpen = false;
        public bool IsOpen => _isOpen;
        protected AnimationCurve _currentCurve;

        #endregion

        #region PRIVATE FIELDS
        protected Quaternion _closedRotation;
        protected Quaternion _openRotation;
        protected Quaternion _reverseOpenRotation;

        protected float _lerpTime = 0f;
        protected bool _isMoving = false;

        #endregion

        #region MONO'S CALLBACK

        protected override void Awake()
        {
            base.Awake();
            _closedRotation = _pivot.localRotation;
            _openRotation = _pivot.localRotation * Quaternion.Euler(_openRotationAngles);
            _reverseOpenRotation = _pivot.localRotation * Quaternion.Euler(-_openRotationAngles);
        }

        protected virtual void Update()
        {
            if (_isMoving)
            {
                float elapsed = Time.time - _rotationStartTime;
                float t = Mathf.Clamp01(elapsed / _rotationDuration);
                float curvedT = _currentCurve.Evaluate(t);

                _pivot.localRotation = Quaternion.Lerp(_closedRotation, _openRotation, _isOpen ? curvedT : 1f - curvedT);

                if (t >= 1f)
                {
                    _isMoving = false;
                }
            }
        }
        #endregion

        #region Register Callbacks
        public override void OnRaycastStart()
        {
            if (_isUnLocked) return;
            base.OnRaycastStart();
        }

        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            if (!_isUnLocked && ServiceLocator.Audio != null && _unlockDoor != null)
                ServiceLocator.Audio.PlayAt(_unlockDoor, this.transform.position);
            _isUnLocked = true;
            CanBeInteracted = true;
            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region LOCALIZATION
        protected override async void UpdateInteractionText()
        {
            LocalizedString targetString = null;
            string bind = null;
            LocalizedString wrapper = null;

            if (!_isUnLocked)
            {
                if (!_hasKeyInInventory)
                {
                    var keyOp = _itemNeededName.GetLocalizedStringAsync();
                    await keyOp.Task;

                    targetString = _missingItemInInventoryTxt;
                    targetString.Arguments = new object[] { new { key = keyOp.Result } };
                }
                else if (_hasKeyInInventory && !_hasKeyOnHands)
                {
                    var keyOp = _itemNeededName.GetLocalizedStringAsync();
                    await keyOp.Task;

                    targetString = _needItemOnHandsTxt;
                    targetString.Arguments = new object[] { new { key = keyOp.Result } };
                }
                else
                {
                    // Clé en main → action pour déverrouiller
                    targetString = _unlockText;
                    bind = InputManager.InteractInInventory_Bind_Name;
                    wrapper = _holdToAction;
                }
            }
            else
            {
                // Porte déverrouillée → ouvrir ou fermer
                targetString = _isOpen ? _closeText : _openText;
                bind = InputManager.Interact_Bind_Name;
                wrapper = _pressToAction;
            }

            InteractionText = await InteractionTextBuilder.Build(
                targetString,
                bind,
                wrapper
            );

            if (IsItemRaycasted)
                InvokeInteractionDescriptionUpdated();
        }

        #endregion

        #region IInteractable
        public override void Interact()
        {
            if (!_isUnLocked)
            {
                if (ServiceLocator.Audio != null && _lockedDoor != null)
                    ServiceLocator.Audio.PlayAt(_lockedDoor, this.transform.position);
                return;
            }
            if (_isOpen)
                Close();
            else
                Open();
            UpdateInteractionText();
            InvokeObjectInteracted();
        }

        public virtual void OnDoorInteracted()
        {
            _rotationStartTime = Time.time;
            _isMoving = true;

            if (IsItemRaycasted)
            {
                // Update the Player's Interactions Raycast data
                InvokeNameUpdated();
                InvokeInteractionDescriptionUpdated();
            }
        }

        #endregion

        #region UNLOCK
        public void SetIsUnlocked(bool value) => _isUnLocked = value;
        #endregion

        #region IDOOR
        public void Open()
        {
            if (!_isUnLocked)
                _isUnLocked = true;

            Vector3 doorForward = transform.forward;
            Vector3 directionToPlayer = (PlayerHandlerData.MainPlayer.Character.transform.position - _pivot.position).normalized;
            float dot = Vector3.Dot(doorForward, directionToPlayer);

            //the player is in front of the door we open normally
            if (dot >= 0f)
                _openRotation = _closedRotation * Quaternion.Euler(_openRotationAngles);
            //the player is behind the door 
            else
                _openRotation = _closedRotation * Quaternion.Euler(-_openRotationAngles);
            _isOpen = true;
            _currentCurve = _openCurve;

            _rotationDuration = 3.5f;
            if (ServiceLocator.Audio != null && _openDoor != null)
            {
                IAudioHandle selectedAudio = ServiceLocator.Audio.PlayAt(_openDoor, this.transform.position);
                _rotationDuration = selectedAudio.SelectedClip.length;
            }
            OnDoorInteracted();
        }

        public void Close()
        {
            _currentCurve = _closeCurve;
            _rotationDuration = 3.5f;
            if (ServiceLocator.Audio != null && _closeDoor != null)
            {
                IAudioHandle selectedAudio = ServiceLocator.Audio.PlayAt(_closeDoor, this.transform.position);
                _rotationDuration = selectedAudio.SelectedClip.length;
            }
            _isOpen = false;
            OnDoorInteracted();
        }
        #endregion
    }
}
