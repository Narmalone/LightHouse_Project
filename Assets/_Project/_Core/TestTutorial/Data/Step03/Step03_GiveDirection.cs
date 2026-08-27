using LightHouse.Core.Audio;
using LightHouse.Core.Utilities;
using LightHouse.Features.Boats;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(
        fileName = "Step03_GiveDirection",
        menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step03_GiveDirection"
    )]
    public class Step03_GiveDirection : TutorialStep
    {
        private enum StepState
        {
            WaitingForFirstChoice,
            FirstChoice,
            FirstRepair,
            WaitingForSecondChoice,
            SecondChoice,
            SecondRepair,
            FinalDialogue,
            Completed
        }

        private TutorialContext _context;

        [Header("Choice Settings")]
        [SerializeField, Min(0.1f)] private float _timerChoiceDuration = 10f;

        [Header("First Choice")]
        [Tooltip("Gauche / Milieu / Droite. Bonne réponse : Droite.")]
        [SerializeField] private LocalizedDialogueAudio _choiceIntroduction;

        [Header("Second Choice")]
        [Tooltip("Deuxième choix si le premier était correct.")]
        [SerializeField] private LocalizedDialogueAudio _captainGoodWayGuide;

        [Tooltip("Deuxième choix si le premier était incorrect.")]
        [SerializeField] private LocalizedDialogueAudio _captainBadWayGuide;

        [Header("Repair")]
        [SerializeField] private LocalizedDialogueAudio _captainBadWayRepairEngine;
        [SerializeField] private LocalizedDialogueAudio _captainBadWayEndSubTutorial;

        [Header("Final Results")]
        [Tooltip("0 erreur.")]
        [SerializeField] private LocalizedDialogueAudio _captainGoodWay;

        [Tooltip("1 erreur.")]
        [SerializeField] private LocalizedDialogueAudio _captainBadWayHalfFalse;

        [Tooltip("2 erreurs.")]
        [SerializeField] private LocalizedDialogueAudio _captainBadWay;

        private Timer _choiceTimer;
        private StepState _state;

        private LocalizedDialogueAudio _currentChoiceDialogue;
        private LocalizedDialogueAudio _expectedFinalDialogue;

        private int _mistakeCount;
        private bool _choiceForcedByTimer;
        private bool _timerActive;

        #region Lifecycle

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;
            _state = StepState.WaitingForFirstChoice;

            _mistakeCount = 0;
            _choiceForcedByTimer = false;
            _timerActive = false;

            _currentChoiceDialogue = null;
            _expectedFinalDialogue = null;

            _choiceTimer = new Timer(_timerChoiceDuration);

            SubscribeEvents();

            ObjectiveManager.Current.SetObjective("Give the captain directions");
        }

        public override void Tick(TutorialContext context, float dt)
        {
            if (_timerActive)
                _choiceTimer.Tick(dt);
        }

        public override void Exit(TutorialContext context)
        {
            StopChoiceTimer();
            UnsubscribeEvents();

            _context = null;
            _currentChoiceDialogue = null;
            _expectedFinalDialogue = null;
        }

        #endregion

        #region Events

        private void SubscribeEvents()
        {
            if (_context.TutoBoat != null)
                _context.TutoBoat.OnChoiceRequired += OnBoatChoiceRequired;

            if (_choiceIntroduction != null)
                _choiceIntroduction.OnChoiceSelected += OnFirstChoiceSelected;

            if (_captainGoodWayGuide != null)
                _captainGoodWayGuide.OnChoiceSelected += OnSecondChoiceSelected;

            if (_captainBadWayGuide != null)
                _captainBadWayGuide.OnChoiceSelected += OnSecondChoiceSelected;

            if (_context.TalkieManager != null)
                _context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

            if (_choiceTimer != null)
                _choiceTimer.OnTimerComplete += OnChoiceTimerFinished;
        }

        private void UnsubscribeEvents()
        {
            if (_context == null)
                return;

            if (_context.TutoBoat != null)
                _context.TutoBoat.OnChoiceRequired -= OnBoatChoiceRequired;

            if (_choiceIntroduction != null)
                _choiceIntroduction.OnChoiceSelected -= OnFirstChoiceSelected;

            if (_captainGoodWayGuide != null)
                _captainGoodWayGuide.OnChoiceSelected -= OnSecondChoiceSelected;

            if (_captainBadWayGuide != null)
                _captainBadWayGuide.OnChoiceSelected -= OnSecondChoiceSelected;

            if (_context.TalkieManager != null)
                _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            if (_choiceTimer != null)
                _choiceTimer.OnTimerComplete -= OnChoiceTimerFinished;
        }

        #endregion

        #region Boat

        private void OnBoatChoiceRequired()
        {
            if (_context.TutoBoat == null)
                return;

            switch (_context.TutoBoat.CurrentChoiceStepIndex)
            {
                case 0:
                    if (_state == StepState.WaitingForFirstChoice)
                        StartFirstChoice();
                    break;

                case 1:
                    if (_state == StepState.WaitingForSecondChoice)
                        StartSecondChoice();
                    break;
            }
        }

        private void SelectBoatDirection(TalkieChoice choice)
        {
            if (_context.TutoBoat == null || choice == null)
                return;

            switch (choice.Index)
            {
                case 0:
                    _context.TutoBoat.ChooseDirection(BoatDirection.Left);
                    break;

                case 1:
                    _context.TutoBoat.ChooseDirection(BoatDirection.Midle);
                    break;

                case 2:
                    _context.TutoBoat.ChooseDirection(BoatDirection.Right);
                    break;

                default:
                    Debug.LogWarning($"[{name}] Invalid choice index: {choice.Index}", this);
                    break;
            }
        }

        #endregion

        #region First Choice

        private void StartFirstChoice()
        {
            _state = StepState.FirstChoice;
            _choiceForcedByTimer = false;
            _currentChoiceDialogue = _choiceIntroduction;

            _context.TalkieManager.Enqueue(_choiceIntroduction);
        }

        private void OnFirstChoiceSelected(TalkieChoice choice)
        {
            if (_state != StepState.FirstChoice || choice == null)
                return;

            StopChoiceTimer();
            SelectBoatDirection(choice);

            // Droite = bon choix.
            bool correct = choice.Index == 2 && !_choiceForcedByTimer;

            if (correct)
            {
                _state = StepState.WaitingForSecondChoice;
                return;
            }

            _mistakeCount++;
            _state = StepState.FirstRepair;

            _context.TutoBoat.Pause();
            StartRepairSequence();
        }

        #endregion

        #region Second Choice

        private void StartSecondChoice()
        {
            _state = StepState.SecondChoice;
            _choiceForcedByTimer = false;

            _currentChoiceDialogue = _mistakeCount == 0
                ? _captainGoodWayGuide
                : _captainBadWayGuide;

            _context.TalkieManager.Enqueue(_currentChoiceDialogue);
        }

        private void OnSecondChoiceSelected(TalkieChoice choice)
        {
            if (_state != StepState.SecondChoice || choice == null)
                return;

            StopChoiceTimer();
            SelectBoatDirection(choice);

            // Milieu = bon choix.
            bool correct = choice.Index == 1 && !_choiceForcedByTimer;

            if (correct)
            {
                StartFinalDialogue(_mistakeCount == 0 ? _captainGoodWay : _captainBadWayHalfFalse);
                return;
            }

            _mistakeCount++;

            // Première erreur au deuxième choix.
            if (_mistakeCount == 1)
            {
                _state = StepState.SecondRepair;
                _context.TutoBoat.Pause();

                StartRepairSequence();
                return;
            }

            // Deux erreurs.
            StartFinalDialogue(_captainBadWay);
        }

        #endregion

        #region Repair

        private void StartRepairSequence()
        {
            _context.Hammer?.SetPickable();

            _context.TalkieManager.Enqueue(_captainBadWayRepairEngine);
            _context.TalkieManager.Enqueue(_captainBadWayEndSubTutorial);
        }

        private void OnRepairFinished()
        {
            if (_state == StepState.FirstRepair)
            {
                _state = StepState.WaitingForSecondChoice;
                _context.TutoBoat.Resume();
                return;
            }

            if (_state == StepState.SecondRepair)
            {
                _context.TutoBoat.Resume();
                StartFinalDialogue(_captainBadWayHalfFalse);
            }
        }

        #endregion

        #region Dialogues

        private void OnDialogueFinished(LocalizedDialogueAudio dialogue)
        {
            if (dialogue == null)
                return;

            // Timer du premier choix.
            if (_state == StepState.FirstChoice && dialogue == _choiceIntroduction)
            {
                StartChoiceTimer();
                return;
            }

            // Timer du deuxième choix.
            if (_state == StepState.SecondChoice && dialogue == _currentChoiceDialogue)
            {
                StartChoiceTimer();
                return;
            }

            // Fin de réparation.
            if ((_state == StepState.FirstRepair || _state == StepState.SecondRepair) && dialogue == _captainBadWayEndSubTutorial)
            {
                OnRepairFinished();
                return;
            }

            // Fin de la step.
            if (_state == StepState.FinalDialogue && dialogue == _expectedFinalDialogue)
            {
                CompleteStep();
            }
        }

        private void StartFinalDialogue(LocalizedDialogueAudio dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogWarning($"[{name}] Final dialogue is null.", this);
                CompleteStep();
                return;
            }

            _state = StepState.FinalDialogue;
            _expectedFinalDialogue = dialogue;

            _context.TalkieManager.Enqueue(dialogue);
        }

        #endregion

        #region Timer

        private void StartChoiceTimer()
        {
            if (_choiceTimer == null)
                return;

            _choiceForcedByTimer = false;
            _timerActive = true;

            _choiceTimer.ResetTimer(false);
            _choiceTimer.StartTimer();
        }

        private void StopChoiceTimer()
        {
            if (_choiceTimer == null)
                return;

            _timerActive = false;

            _choiceTimer.StopTimer();
            _choiceTimer.ResetTimer(false);
        }

        private void OnChoiceTimerFinished()
        {
            if (_state != StepState.FirstChoice && _state != StepState.SecondChoice)
                return;

            _timerActive = false;
            _choiceForcedByTimer = true;

            // Timeout = Milieu.
            if (!_context.TalkieManager.ForceChoice(1))
                Debug.LogWarning($"[{name}] Could not force Middle choice.", this);
        }

        #endregion

        #region Complete

        private void CompleteStep()
        {
            if (_state == StepState.Completed)
                return;

            StopChoiceTimer();

            _state = StepState.Completed;
            _expectedFinalDialogue = null;

            ObjectiveManager.Current.CompleteObjective();
            IsComplete = true;
        }

        #endregion
    }
}