using LightHouse.Core.Audio;
using LightHouse.Core.Player;
using LightHouse.Core.Utilities;
using LightHouse.Features.Boats;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step03_GiveDirection", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step03_GiveDirection")]

    public class Step03_GiveDirection : TutorialStep
    {
        #region REFERENCES

        private TutorialContext _context;

        #endregion


        #region CONFIGURATION

        [Header("Choice Settings")]

        [SerializeField] private float _timerChoiceDuration = 10f;


        [Header("Choice Introduction")]

        [SerializeField] private LocalizedDialogueAudio _choiceIntroduction;


        [Header("Bad Direction")]

        // Dialogue played when the player makes a wrong choice
        // during the first direction selection.
        [SerializeField] private LocalizedDialogueAudio _captainBadWayGuide;

        // Dialogue played when the player makes a wrong choice
        // during the second direction selection.
        [SerializeField] private LocalizedDialogueAudio _captainBadWay;

        // Dialogue played when the player gets the first choice right
        // but makes a mistake on the second choice.
        [SerializeField] private LocalizedDialogueAudio _captainBadWayHalfFalse;

        // Dialogue explaining that the engine needs to be repaired.
        [SerializeField] private LocalizedDialogueAudio _captainBadWayRepairEngine;

        // Final dialogue of the failure path.
        // This ends this tutorial sub-step.
        [SerializeField] private LocalizedDialogueAudio _captainBadWayEndSubTutorial;


        [Header("Good Direction")]

        // Dialogue played after the player makes the correct
        // first direction choice.
        [SerializeField] private LocalizedDialogueAudio _captainGoodWayGuide;

        // Dialogue played when both direction choices are correct.
        [SerializeField] private LocalizedDialogueAudio _captainGoodWay;

        #endregion


        #region RUNTIME VARIABLES

        private Timer _forcedChoice;

        /*
         * Tick() is called every frame by the Tutorial system.
         *
         * We only want the choice timer to progress while an actual
         * choice is currently waiting for the player.
         */
        private bool _tickActivated;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            /*
             * This tutorial step is essentially a two-stage decision tree:
             *
             *                         First choice
             *                              │
             *                ┌─────────────┴─────────────┐
             *                │                           │
             *             GOOD                        BAD
             *                │                           │
             *         Second choice              Second choice
             *                │                           │
             *          ┌─────┴─────┐               ┌─────┴─────┐
             *          │           │               │           │
             *        GOOD        BAD             GOOD        BAD
             *          │           │               │           │
             *       Success    Half False        BAD         BAD
             *
             * Each choice has a limited amount of time.
             * If the player doesn't answer before the timer expires,
             * the TalkieManager automatically selects choice 1.
             *
             * The dialogue system is responsible for presenting the choices,
             * while this TutorialStep is responsible for interpreting the
             * player's answers and deciding which tutorial branch to follow.
             */

            ObjectiveManager.Current.SetObjective("Give the captain directions");

            _forcedChoice = new Timer(_timerChoiceDuration);

            SubscribeToEvents();
        }


        public override void Tick(TutorialContext context, float dt)
        {
            if (!_tickActivated)
                return;

            _forcedChoice.Tick(dt);
        }


        public override void Exit(TutorialContext context)
        {
            UnsubscribeFromEvents();
        }

        #endregion


        #region INITIALIZATION

        private void SubscribeToEvents()
        {
            _context.TutoBoat.OnChoiceRequired += TutoBoat_OnChoiceRequired;

            _choiceIntroduction.OnChoiceSelected += Captain_OnFirstChoiceSelected;

            _context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

            _forcedChoice.OnTimerComplete += TimerChoiceDuration_OnTimerComplete;
        }

        private void UnsubscribeFromEvents()
        {
            _context.TutoBoat.OnChoiceRequired -= TutoBoat_OnChoiceRequired;

            _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            _choiceIntroduction.OnChoiceSelected -= Captain_OnFirstChoiceSelected;

            _captainGoodWayGuide.OnChoiceSelected -= CaptainGoodWayGuide_OnChoiceSelected;

            _captainBadWayGuide.OnChoiceSelected -= CaptainBadWayGuide_OnChoiceSelected;

            _forcedChoice.OnTimerComplete -= TimerChoiceDuration_OnTimerComplete;
        }

        #endregion


        #region DIALOGUE FLOW

        private void OnDialogueFinished(LocalizedDialogueAudio dialogue)
        {
            /*
             * Dialogue completion is used as the synchronization point
             * between the dialogue system and the tutorial system.
             *
             * Depending on which dialogue has just finished, we either:
             *
             * - complete the tutorial immediately;
             * - complete the current objective and start the repair sequence;
             * - or wait for the next choice.
             */

            if (dialogue == _captainBadWayEndSubTutorial)
            {
                IsComplete = true;
            }
            else if (dialogue == _captainGoodWay)
            {
                ObjectiveManager.Current.CompleteObjective();

                IsComplete = true;
            }
            else if (dialogue == _captainBadWay || dialogue == _captainBadWayHalfFalse)
            {
                ObjectiveManager.Current.CompleteObjective();

                _context.TalkieManager.Enqueue(_captainBadWayRepairEngine);
                _context.TalkieManager.Enqueue(_captainBadWayEndSubTutorial);

                _context.Hammer.SetPickable();
            }
            /*else if ()
            {
                ObjectiveManager.Current.CompleteObjective();

                _context.TalkieManager.Enqueue(_captainBadWayRepairEngine);
                _context.TalkieManager.Enqueue(_captainBadWayEndSubTutorial);

                _context.Hammer.SetPickable();
            }*/
        }

        #endregion


        #region CHOICE TIMER

        private void TimerChoiceDuration_OnTimerComplete()
        {
            /*
             * The player didn't make a choice before the allowed time.
             *
             * Instead of leaving the tutorial blocked indefinitely,
             * the TalkieManager automatically selects choice 1.
             *
             * The actual consequences of that choice are still handled
             * normally by the corresponding OnChoiceSelected callback.
             */

            _context.TalkieManager.ForceChoice(1);
        }


        private void StopChoiceTimer()
        {
            /*
             * Once the player has made a choice, the current timer must
             * immediately stop.
             *
             * ResetTimer(false) clears the timer's elapsed state without
             * automatically starting it again.
             */

            _tickActivated = false;

            _forcedChoice.ResetTimer(false);
            _forcedChoice.StopTimer();
        }

        #endregion


        #region FIRST CHOICE

        private void Captain_OnFirstChoiceSelected(TalkieChoice choice)
        {
            StopChoiceTimer();
            _context.TutoBoat.Resume();

            if (choice.Index == 2)
            {
                _context.TutoBoat.ChooseDirection(BoatDirection.Right);
            }
            else
            {
                if (choice.Index == 0)
                {
                    _context.TutoBoat.ChooseDirection(BoatDirection.Left);
                }
                else if (choice.Index == 1)
                {
                    _context.TutoBoat.ChooseDirection(BoatDirection.Midle);
                }
            }
        }

        #endregion


        #region BAD WAY CHOICE

        private void CaptainBadWayGuide_OnChoiceSelected(TalkieChoice choice)
        {
            StopChoiceTimer();
            _context.TutoBoat.Resume();

            if (choice.Index == 1)
            {
                _context.TutoBoat.ChooseDirection(BoatDirection.Midle);
                _context.TalkieManager.Enqueue(_captainBadWayHalfFalse);
            }
            else
            {
                if (choice.Index == 0)
                {
                    _context.TutoBoat.ChooseDirection(BoatDirection.Left);
                }
                else if (choice.Index == 2)
                {
                    _context.TutoBoat.ChooseDirection(BoatDirection.Right);
                }

                _context.TalkieManager.Enqueue(_captainBadWay);
            }
        }

        #endregion


        #region GOOD WAY CHOICE

        private void CaptainGoodWayGuide_OnChoiceSelected(TalkieChoice choice)
        {
            StopChoiceTimer();
            _context.TutoBoat.Resume();

            if (choice.Index == 1)
            {
                _context.TutoBoat.ChooseDirection(BoatDirection.Midle);
                _context.TalkieManager.Enqueue(_captainGoodWay);
            }
            else
            {
                if (choice.Index == 0)
                {
                    _context.TutoBoat.ChooseDirection(BoatDirection.Left);
                }
                else if (choice.Index == 2)
                {
                    _context.TutoBoat.ChooseDirection(BoatDirection.Right);
                }

                _context.TalkieManager.Enqueue(_captainBadWay);
            }
        }

        #endregion


        #region CHOICE REQUIRED
        private void TutoBoat_OnChoiceRequired()
        {
            _context.TutoBoat.Pause();

            _tickActivated = true;
            _forcedChoice.StartTimer();

            switch (_context.TutoBoat.CurrentChoiceStepIndex)
            {
                case 0:
                    _context.TalkieManager.Enqueue(_choiceIntroduction);

                    break;

                case 1:
                    if (_context.TutoBoat.ChosenDirection == BoatDirection.Right)
                    {
                        _context.TalkieManager.Enqueue(_captainGoodWayGuide);
                        _captainGoodWayGuide.OnChoiceSelected += CaptainGoodWayGuide_OnChoiceSelected;
                    }
                    else if (_context.TutoBoat.ChosenDirection != BoatDirection.Right)
                    {
                        _context.TalkieManager.Enqueue(_captainBadWayGuide);
                        _captainBadWayGuide.OnChoiceSelected += CaptainBadWayGuide_OnChoiceSelected;
                    }

                    break;

            }
        }
        #endregion
    }
}