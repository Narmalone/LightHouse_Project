using LightHouse.Core.Audio;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step05_GetOnTheDeck", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step05_GetOnTheDeck")]
    public class Step05_GetOnTheDeck : TutorialStep
    {
        [Header("Dialogues")]
        [SerializeField] private LocalizedDialogueAudio _arrivingSoon;
        [SerializeField] private LocalizedDialogueAudio _onDock;
        [SerializeField] private LocalizedDialogueAudio _onDepart;

        private TutorialContext _context;
        private Collider _boatCollider;
        private Trigger _trigger;

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            SubscribeEvents();

            _context.TalkieManager.Enqueue(_arrivingSoon);
        }

        public override void Exit(TutorialContext context)
        {
            UnsubscribeEvents();
            UnsubscribeTrigger();

            _context = null;
            _boatCollider = null;

            base.Exit(context);
        }

        private void TutoBoat_OnPathCompleted()
        {
            ObjectiveManager.Current.SetObjective("Get on the dock.");

            _context.TalkieManager.Enqueue(_onDock);

            SetupDockTrigger();
        }

        private void SetupDockTrigger()
        {
            _boatCollider = _context.RightMiddleBoatCollider;

            if (_boatCollider == null)
            {
                Debug.LogError("[Step05_GetOnTheDeck] RightMiddleBoatCollider is null.");
                return;
            }

            _trigger = _boatCollider.GetComponent<Trigger>();

            if (_trigger == null)
            {
                Debug.LogError("[Step05_GetOnTheDeck] No Trigger component found on RightMiddleBoatCollider.");
                return;
            }

            _boatCollider.isTrigger = true;

            _trigger.OnEntered -= OnPlayerEntered;
            _trigger.OnEntered += OnPlayerEntered;
        }

        private void OnPlayerEntered(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            ObjectiveManager.Current.CompleteObjective();

            _context.TalkieManager.Enqueue(_onDepart);

            UnsubscribeTrigger();

            if (_boatCollider != null)
            {
                _boatCollider.isTrigger = false;
                _boatCollider.enabled = false;
            }
        }
        private void TalkieManager_OnDialogueFinished(LocalizedDialogueAudio obj)
        {
            if (obj == _onDepart)
            {
                IsComplete = true;
            }
        }

        private void SubscribeEvents()
        {
            if (_context?.TutoBoat == null)
                return;

            _context.TutoBoat.OnPathCompleted -= TutoBoat_OnPathCompleted;
            _context.TutoBoat.OnPathCompleted += TutoBoat_OnPathCompleted;
            _context.TalkieManager.OnDialogueFinished += TalkieManager_OnDialogueFinished;
        }


        private void UnsubscribeEvents()
        {
            if (_context?.TutoBoat == null)
                return;

            _context.TutoBoat.OnPathCompleted -= TutoBoat_OnPathCompleted;
            _context.TalkieManager.OnDialogueFinished -= TalkieManager_OnDialogueFinished;
        }

        private void UnsubscribeTrigger()
        {
            if (_trigger == null)
                return;

            _trigger.OnEntered -= OnPlayerEntered;
            _trigger = null;
        }
    }
}