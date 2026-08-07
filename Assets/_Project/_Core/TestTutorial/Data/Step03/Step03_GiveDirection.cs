using LightHouse.Core.Audio;
using LightHouse.Core.Utilities;
using LightHouse.Features.Boats;
using System.Collections;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step03_GiveDirection", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step03_GiveDirection")]

    public class Step03_GiveDirection : TutorialStep
    {
        #region REFERENCES

        private TutorialContext _context;
        private MonoBehaviour _routineBehaviour;

        #endregion


        #region CONFIGURATION

        [Header("Choice Settings")]
        [SerializeField] private float _timerChoiceDuration = 10f;


        [Header("Dialogues")]

        [SerializeField] private LocalizedDialogueAudio _choiceIntroduction;


        [Header("Wrong Choice")]

        [SerializeField] private LocalizedDialogueAudio _captainBadWayGuide;
        [SerializeField] private LocalizedDialogueAudio _captainBadWay;
        [SerializeField] private LocalizedDialogueAudio _captainBadWayHalfFalse;
        [SerializeField] private LocalizedDialogueAudio _captainBadWayRepairEngine;
        [SerializeField] private LocalizedDialogueAudio _captainBadWayEndSubTutorial;


        [Header("Good Choice")]

        [SerializeField] private LocalizedDialogueAudio _captainGoodWayGuide;
        [SerializeField] private LocalizedDialogueAudio _captainGoodWay;

        #endregion


        #region RUNTIME

        private Timer _forcedChoice;

        private bool _tickActivated;

        private bool _wrongChoice;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            _routineBehaviour = context.Flow;

            _forcedChoice = new Timer(_timerChoiceDuration);

            _wrongChoice = false;

            ObjectiveManager.Current.SetObjective("Give the captain directions");

            SubscribeEvents();
        }

        public override void Tick(TutorialContext context, float dt)
        {
            if (_tickActivated)
                _forcedChoice.Tick(dt);
        }

        public override void Exit(TutorialContext context)
        {
            UnsubscribeEvents();
        }

        #endregion



        #region EVENTS

        private void SubscribeEvents()
        {
            _context.TutoBoat.OnChoiceRequired += OnBoatChoiceRequired;

            _choiceIntroduction.OnChoiceSelected += OnFirstChoiceSelected;

            _context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

            _forcedChoice.OnTimerComplete += OnChoiceTimerFinished;
        }

        private void UnsubscribeEvents()
        {
            _context.TutoBoat.OnChoiceRequired -= OnBoatChoiceRequired;

            _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            _choiceIntroduction.OnChoiceSelected -= OnFirstChoiceSelected;

            _captainGoodWayGuide.OnChoiceSelected -= OnSecondGoodChoiceSelected;

            _captainBadWayGuide.OnChoiceSelected -= OnSecondBadChoiceSelected;

            _forcedChoice.OnTimerComplete -= OnChoiceTimerFinished;
        }

        #endregion



        #region DIALOGUE FLOW

        private void OnDialogueFinished(LocalizedDialogueAudio dialogue)
        {
            if (dialogue == _captainGoodWay)
            {
                CompleteTutorial();

            }
            else if (dialogue == _captainBadWay || dialogue == _captainBadWayHalfFalse)
            {
                HandleWrongChoice();
            }
            else if (dialogue == _captainBadWayEndSubTutorial)
            {
                IsComplete = true;
            }
        }

        private void CompleteTutorial()
        {
            ObjectiveManager.Current.CompleteObjective();
            
            IsComplete = true;
        }

        private void HandleWrongChoice()
        {
            ObjectiveManager.Current.CompleteObjective();

            _wrongChoice = true;

            _context.TalkieManager.Enqueue(_captainBadWayRepairEngine);
            _context.TalkieManager.Enqueue(_captainBadWayEndSubTutorial);

            _context.Hammer.SetPickable();
        }

        #endregion



        #region BOAT CHOICES

        private void OnBoatChoiceRequired()
        {
            _context.TutoBoat.Pause();

            StartChoiceTimer();


            switch (_context.TutoBoat.CurrentChoiceStepIndex)
            {
                case 0:
                    StartFirstChoice();
                    break;

                case 1:
                    StartSecondChoice();
                    break;
            }
        }

        private void StartFirstChoice()
        {
            _context.TalkieManager.Enqueue(_choiceIntroduction);
        }

        private void StartSecondChoice()
        {
            if (_context.TutoBoat.ChosenDirection == BoatDirection.Right)
            {

                _context.TalkieManager.Enqueue(_captainGoodWayGuide);

                _captainGoodWayGuide.OnChoiceSelected += OnSecondGoodChoiceSelected;
            }
            else
            {
                _context.TalkieManager.Enqueue(_captainBadWayGuide);

                _captainBadWayGuide.OnChoiceSelected += OnSecondBadChoiceSelected;
            }
        }

        private IEnumerator WaitForTimer(WaitForSeconds waitForSeconds)
        {
            yield return waitForSeconds;
        }

        #endregion



        #region CHOICE RESULT

        private void OnFirstChoiceSelected(TalkieChoice choice)
        {
            StopChoiceTimer();

            SelectBoatDirection(choice);


            _context.TutoBoat.Resume();
        }

        private void OnSecondGoodChoiceSelected(TalkieChoice choice)
        {
            StopChoiceTimer();


            SelectBoatDirection(choice);


            if (choice.Index == 1)
            {
                _context.TalkieManager.Enqueue(_captainGoodWay);
            }
            else
            {
                _wrongChoice = true;
                _routineBehaviour.StartCoroutine(WaitForTimer(new WaitForSeconds(12f)));
                _context.TalkieManager.Enqueue(_captainBadWay);
            }


            _context.TutoBoat.Resume();
        }

        private void OnSecondBadChoiceSelected(TalkieChoice choice)
        {
            StopChoiceTimer();


            SelectBoatDirection(choice);


            if (choice.Index == 1)
            {
                _context.TalkieManager.Enqueue(_captainBadWayHalfFalse);
            }
            else
            {
                _context.TalkieManager.Enqueue(_captainBadWay);
            }


            _context.TutoBoat.Resume();
        }

        private void SelectBoatDirection(TalkieChoice choice)
        {
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
            }
        }

        #endregion



        #region TIMER

        private void StartChoiceTimer()
        {
            _tickActivated = true;

            _forcedChoice.StartTimer();
        }

        private void StopChoiceTimer()
        {
            _tickActivated = false;

            _forcedChoice.ResetTimer(false);

            _forcedChoice.StopTimer();
        }

        private void OnChoiceTimerFinished()
        {
            _context.TalkieManager.ForceChoice(1);
        }

        #endregion
    }
}