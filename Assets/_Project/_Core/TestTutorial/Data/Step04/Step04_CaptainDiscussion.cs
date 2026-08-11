using LightHouse.Core.Audio;
using LightHouse.Core.Utilities;
using System;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step04_CaptainDiscussion", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step04_CaptainDiscussion")]
    public class Step04_CaptainDiscussion : TutorialStep
    {
        #region REFERENCES

        private TutorialContext _context;
        private MonoBehaviour _routineBehaviour;

        #endregion


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
        [SerializeField] private LocalizedDialogueAudio _Warning;
        [SerializeField] private LocalizedDialogueAudio _forcedDialogueStop;



        [Header("Question Timer")]

        [SerializeField] private float _timeToAsk = 10f;


        [Header("Dialogue Delays")]

        [SerializeField] private float _delayBetweenDialogues = 2f;

        #endregion


        #region RUNTIME

        private bool _tickActivated;
        private bool _questionStop;

        private int _question1Count;
        private int _question2Count;
        private int _question3Count;

        private Timer _questionTimer;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;
            _routineBehaviour = context.Flow;

            InitializeRuntime();

            SubscribeEvents();

            StartDialogueSequence();
        }


        public override void Tick(TutorialContext context, float dt)
        {
            if (!_tickActivated)
                return;

            _questionTimer.Tick(dt);
        }


        public override void Exit(TutorialContext context)
        {
            UnsubscribeEvents();
        }

        #endregion


        #region INITIALIZATION

        private void InitializeRuntime()
        {
            _tickActivated = false;

            _questionTimer = new Timer(_timeToAsk);

            _tickActivated = false;
            _questionStop = false;

            _question1Count = 0;
            _question2Count = 0;
            _question3Count = 0;

            _questionTimer = new Timer(_timeToAsk);
        }


        private void StartDialogueSequence()
        {
            _context.TalkieManager.Enqueue(_captainTalking_1);
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
            _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            _questionTimer.OnTimerComplete -= OnQuestionTimerFinished;

            _askQuestions.OnChoiceSelected += OnChoiceSelected;

            _askNextQuestions.OnChoiceSelected += OnChoiceSelected;
        }

        #endregion


        #region DIALOGUE FLOW

        private void OnDialogueFinished(LocalizedDialogueAudio dialogue)
        {
            if (dialogue == _captainTalking_1)
            {
                _context.TalkieManager.Enqueue(_captainTalking_2);
            }
            else if (dialogue == _captainTalking_2)
            {
                _context.TalkieManager.Enqueue(_captainTalking_3);
            }
            else if (dialogue == _captainTalking_3)
            {
                StartDelayedQuestionSequence();
            }
            else if (dialogue == _endQuestionsDialogue)
            {
                IsComplete = true;
            }
            else if (dialogue == _forcedDialogueStop)
            {
                IsComplete = true;
            }
        }


        private void StartDelayedQuestionSequence()
        {
            _context.TalkieManager.Enqueue(_askQuestions);
            StartQuestionTimer();
        }


        private void OnChoiceSelected(TalkieChoice choice)
        {
            StopQuestionTimer();

            if (_questionStop)
                return;

            switch (choice.Index)
            {
                case 0:
                    HandleQuestion(ref _question1Count, _captainQuestion_1
                    );
                    break;

                case 1:
                    HandleQuestion(ref _question2Count, _captainQuestion_2
                    );
                    break;

                case 2:
                    HandleQuestion(ref _question3Count, _captainQuestion_3
                    );
                    break;

                case 3:
                    EndQuestions();
                    break;
            }
        }

        #endregion

        private void HandleQuestion(ref int questionCount, LocalizedDialogueAudio questionDialogue)
        {
            questionCount++;

            // Première fois
            if (questionCount == 1)
            {
                _context.TalkieManager.Enqueue(questionDialogue);
                _context.TalkieManager.Enqueue(_askNextQuestions);

                StartQuestionTimer();
                return;
            }

            // Deuxième fois
            if (questionCount == 2)
            {
                _context.TalkieManager.Enqueue(_Warning);
                _context.TalkieManager.Enqueue(_askNextQuestions);

                StartQuestionTimer();
                return;
            }

            // Troisième fois
            ForceEndQuestions();
        }

        private void ForceEndQuestions()
        {
            _questionStop = true;

            StopQuestionTimer();

            _context.TalkieManager.Enqueue(_forcedDialogueStop);
        }

        private void EndQuestions()
        {
            _questionStop = true;

            StopQuestionTimer();

            _context.TalkieManager.Enqueue(_endQuestionsDialogue);
        }

        private void StopQuestionTimer()
        {
            _tickActivated = false;

            _questionTimer.ResetTimer(false);
            _questionTimer.StopTimer();
        }


        #region QUESTION TIMER

        private void StartQuestionTimer()
        {
            _tickActivated = true;

            _questionTimer.ResetTimer(false);
            _questionTimer.StartTimer();
        }


        private void OnQuestionTimerFinished()
        {
            _tickActivated = false;

            _context.TalkieManager.ForceChoice(4);
        }

        #endregion
    }
}