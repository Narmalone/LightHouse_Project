using LightHouse.Core.Audio;
using LightHouse.Core.Utilities;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(
        fileName = "Step04_CaptainDiscussion",
        menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step04_CaptainDiscussion")]
    public class Step04_CaptainDiscussion : TutorialStep
    {
        #region CONFIGURATION

        [Header("Captain Dialogues")]
        [SerializeField] private LocalizedDialogueAudio _captainTalking_1;
        [SerializeField] private LocalizedDialogueAudio _captainTalking_2;
        [SerializeField] private LocalizedDialogueAudio _captainTalking_3;

        [SerializeField] private LocalizedDialogueAudio _captainQuestion_1;
        [SerializeField] private LocalizedDialogueAudio _captainQuestion_2;
        [SerializeField] private LocalizedDialogueAudio _captainQuestion_3;

        [SerializeField] private LocalizedDialogueAudio _askQuestions;
        [SerializeField] private LocalizedDialogueAudio _askNextQuestions;

        [SerializeField] private LocalizedDialogueAudio _endQuestionsDialogue;
        [SerializeField] private LocalizedDialogueAudio _warning;
        [SerializeField] private LocalizedDialogueAudio _forcedDialogueStop;

        [Header("Question Timer")]
        [SerializeField] private float _timeToAsk = 10f;

        #endregion


        #region RUNTIME

        private TutorialContext _context;

        private Timer _questionTimer;

        private bool _tickActivated;
        private bool _questionStop;

        private int _question1Count;
        private int _question2Count;
        private int _question3Count;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            InitializeRuntime();
            SubscribeEvents();

            _context.TalkieManager.Enqueue(_captainTalking_1);
        }

        public override void Tick(TutorialContext context, float dt)
        {
            if (_tickActivated)
                _questionTimer.Tick(dt);
        }

        public override void Exit(TutorialContext context)
        {
            StopQuestionTimer();
            UnsubscribeEvents();

            _context = null;

            base.Exit(context);
        }

        #endregion


        #region INITIALIZATION

        private void InitializeRuntime()
        {
            _tickActivated = false;
            _questionStop = false;

            _question1Count = 0;
            _question2Count = 0;
            _question3Count = 0;

            _questionTimer = new Timer(_timeToAsk);
        }

        #endregion


        #region EVENTS

        private void SubscribeEvents()
        {
            _context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

            _questionTimer.OnTimerComplete += OnQuestionTimerFinished;

            _askQuestions.OnChoiceSelected += OnChoiceSelected;
            _askNextQuestions.OnChoiceSelected += OnChoiceSelected;
        }

        private void UnsubscribeEvents()
        {
            if (_context == null)
                return;

            _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            if (_questionTimer != null)
                _questionTimer.OnTimerComplete -= OnQuestionTimerFinished;

            _askQuestions.OnChoiceSelected -= OnChoiceSelected;
            _askNextQuestions.OnChoiceSelected -= OnChoiceSelected;
        }

        #endregion


        #region DIALOGUE FLOW

        private void OnDialogueFinished(LocalizedDialogueAudio dialogue)
        {
            if (dialogue == _captainTalking_1)
            {
                _context.TalkieManager.Enqueue(_captainTalking_2);
                return;
            }

            if (dialogue == _captainTalking_2)
            {
                _context.TalkieManager.Enqueue(_captainTalking_3);
                return;
            }

            if (dialogue == _captainTalking_3)
            {
                StartQuestions();
                return;
            }

            if (dialogue == _endQuestionsDialogue ||
                dialogue == _forcedDialogueStop)
            {
                IsComplete = true;
            }
        }

        private void StartQuestions()
        {
            _context.TalkieManager.Enqueue(_askQuestions);
            StartQuestionTimer();
        }

        private void OnChoiceSelected(TalkieChoice choice)
        {
            if (_questionStop)
                return;

            StopQuestionTimer();

            switch (choice.Index)
            {
                case 0:
                    HandleQuestion(
                        ref _question1Count,
                        _captainQuestion_1);
                    break;

                case 1:
                    HandleQuestion(
                        ref _question2Count,
                        _captainQuestion_2);
                    break;

                case 2:
                    HandleQuestion(
                        ref _question3Count,
                        _captainQuestion_3);
                    break;

                case 3:
                    EndQuestions(_endQuestionsDialogue);
                    break;
            }
        }

        private void HandleQuestion(
            ref int questionCount,
            LocalizedDialogueAudio questionDialogue)
        {
            questionCount++;

            switch (questionCount)
            {
                case 1:
                    _context.TalkieManager.Enqueue(questionDialogue);
                    QueueNextQuestions();
                    break;

                case 2:
                    _context.TalkieManager.Enqueue(_warning);
                    QueueNextQuestions();
                    break;

                default:
                    EndQuestions(_forcedDialogueStop);
                    break;
            }
        }

        private void QueueNextQuestions()
        {
            _context.TalkieManager.Enqueue(_askNextQuestions);
            StartQuestionTimer();
        }

        private void EndQuestions(LocalizedDialogueAudio endDialogue)
        {
            if (_questionStop)
                return;

            _questionStop = true;

            StopQuestionTimer();

            _context.TalkieManager.Enqueue(endDialogue);
        }

        #endregion


        #region QUESTION TIMER

        private void StartQuestionTimer()
        {
            _questionTimer.ResetTimer(false);
            _questionTimer.StartTimer();

            _tickActivated = true;
        }

        private void StopQuestionTimer()
        {
            _tickActivated = false;

            if (_questionTimer == null)
                return;

            _questionTimer.ResetTimer(false);
            _questionTimer.StopTimer();
        }

        private void OnQuestionTimerFinished()
        {
            _tickActivated = false;

            _context.TalkieManager.ForceChoice(4);
        }

        #endregion
    }
}