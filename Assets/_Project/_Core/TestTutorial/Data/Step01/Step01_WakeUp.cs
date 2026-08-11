using LightHouse.Core.Audio;
using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;
using LightHouse.Core.Player;
using LightHouse.Core.Utilities;
using LightHouse.Features.TimeOfDay.TimeCore;
using LightHouse.Features.Tutorial;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step01_WakeUp", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step01_WakeUp")]

    public class Step01_WakeUp : TutorialStep
    {
        #region REFERENCES

        private TutorialContext _context;
        private MonoBehaviour _routineBehaviour;

        #endregion


        #region CONFIGURATION

        [Header("Wake Up")]

        [SerializeField] private LocalizedString _wakeUpText;

        [SerializeField] private LocalizedString _pressToAction;

        [SerializeField] private float _delayBeforePlayerCanInputDuration = 5f;


        [Header("Player Movement")]

        [SerializeField] private float _timeWhenPlayerNotMoving = 10f;


        [Header("Captain Dialogues")]

        [SerializeField] private LocalizedDialogueAudio _captainInitialDialogue;

        [SerializeField] private LocalizedDialogueAudio _captainReminderToMoveDialogue;

        #endregion


        #region RUNTIME VARIABLES

        private WaitForSeconds _delayPlayerWakeAfterPagerBip;

        private string _wakeUpInteractionText;

        private Timer _timerWhenPlayerNotMoving;

        private bool _isPlayerHasToMove = false;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;
            _routineBehaviour = context.Flow;

            TimeHandlerData.TimeSpeed = 0.0f;

            /*
             * The wake-up step is composed of several independent systems:
             *
             * 1. The player starts in complete darkness.
             * 2. The wake-up camera takes control of the view.
             * 3. A delay is used before the pager starts beeping.
             * 4. After another delay, the player receives the instruction
             *    explaining how to wake up.
             * 5. Once the player wakes up, the boat resumes and the
             *    captain's dialogue starts.
             * 6. After the dialogue, movement is unlocked and becomes
             *    the tutorial objective.
             *
             * The initialization is therefore split into several methods
             * to keep Enter() focused on the overall sequence.
             */

            InitializeRuntimeVariables();
            SetupWakeUpSequence();

            SubscribeToEvents();

            SetupTutorialObjects();
        }


        public override void Tick(TutorialContext context, float dt)
        {
            // The inactivity timer is only relevant once the player
            // has been instructed to move.
            if (!_isPlayerHasToMove)
                return;

            _timerWhenPlayerNotMoving.Tick(dt);
        }


        public override void Exit(TutorialContext context)
        {
            // Make sure no event remains subscribed when this tutorial
            // step is left. This is especially important for a TutorialStep
            // that can be entered/exited multiple times.
            UnsubscribeFromEvents();
        }

        #endregion


        #region INITIALIZATION

        private void InitializeRuntimeVariables()
        {
            /*
             * WaitForSeconds objects are created once and reused by the
             * wake-up sequence instead of creating the same delay every time.
             *
             * The movement timer is also created here because it depends
             * on the configurable inactivity duration.
             */

            _delayPlayerWakeAfterPagerBip =
                new WaitForSeconds(_delayBeforePlayerCanInputDuration);

            _isPlayerHasToMove = false;

            _timerWhenPlayerNotMoving =
                new Timer(_timeWhenPlayerNotMoving);
        }


        private void SetupWakeUpSequence()
        {
            /*
             * At the beginning of the tutorial the player should not see
             * the normal gameplay view.
             *
             * The black screen hides the scene while the WakeUpCam is given
             * a very high priority so Cinemachine selects it as the active
             * camera.
             *
             * We then start a delayed sequence:
             *
             *     Enter()
             *        ↓
             *     4 seconds
             *        ↓
             *     Pager starts beeping
             *        ↓
             *     5 seconds
             *        ↓
             *     Wake-up instruction appears
             *
             * This sequence is asynchronous because the tutorial must
             * continue running while these delays are active.
             */

            BlackScreenController.Current.StartFade(1f, -1f);

            // Give priority to the wake-up camera.
            _context.WakeUpCam.Priority = 1000;

            // Wait before playing the pager bip.
            _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine( new WaitForSeconds(4f), OnPagerDelayEnded)
            );
        }


        private void SetupTutorialObjects()
        {
            /*
             * While the player is waking up, gameplay interactions that
             * belong to later tutorial steps must remain unavailable.
             *
             * These objects will become available again when their
             * corresponding tutorial step starts.
             */

            _context.TutoBoat.Pause();
            _context.Binocular.SetUnpickable();
            _context.Hammer.SetUnpickable();
        }

        #endregion


        #region EVENTS

        private void SubscribeToEvents()
        {
            /*
             * We listen to dialogue completion instead of assuming how long
             * a dialogue will last.
             *
             * This is important because dialogue duration can change
             * depending on the localized audio without affecting the
             * tutorial logic.
             */

            _context.TalkieManager.OnDialogueFinished += TalkieManager_OnDialogueFinished;
        }


        private void UnsubscribeFromEvents()
        {
            /*
             * Every subscription made by this tutorial step is removed here.
             *
             * Without this cleanup, the TutorialStep could continue reacting
             * to input or dialogue events after it has already been completed,
             * potentially causing duplicate callbacks when the step is
             * entered again.
             */

            _context.TalkieManager.OnDialogueFinished -= TalkieManager_OnDialogueFinished;

            if (_timerWhenPlayerNotMoving != null)
            {
                _timerWhenPlayerNotMoving.OnTimerComplete -= TimerWhenPlayerNotMoving_OnTimerComplete;
            }

            InputManager.PIA.Player.Move.performed -= MovePerformed;
            InputManager.PIA.Player.Jump.performed -= JumpPerformed;
        }

        #endregion


        #region WAKE UP SEQUENCE

        private void OnPagerDelayEnded()
        {
            /*
             * The first delay has ended, so the pager starts beeping.
             *
             * We deliberately use another coroutine instead of immediately
             * displaying the wake-up instruction. This creates a small
             * gameplay sequence where the player hears the pager before
             * being told how to wake up.
             */

            _context.TalkieManager.Bip();

            _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine( _delayPlayerWakeAfterPagerBip, OnFirstDelayEnded)
            );
        }


        private async void OnFirstDelayEnded()
        {
            /*
             * The player can now be informed about the input required to wake up.
             *
             * The text is generated asynchronously because InteractionTextBuilder
             * resolves localized strings and inserts the current input binding.
             *
             * Example:
             *
             *     "Hold [SPACE] to wake up"
             *
             * The actual key/button is therefore not hardcoded and can change
             * according to the player's current input configuration.
             */

            string inputName = InputManager.Jump_Bind_Name;

            _wakeUpInteractionText = await InteractionTextBuilder.Build_Hold_To_Action(_wakeUpText, inputName, _pressToAction);

            // Display the generated localized interaction text.
            BlackScreenController.Current.SetWakeUpText(_wakeUpInteractionText);

            // Wait for the player to perform the wake-up input.
            InputManager.PIA.Player.Jump.performed += JumpPerformed;

            // Fade the instruction in.
            BlackScreenController.Current.FadeWakeUpText(1f, 2f, null, null);
        }


        private void JumpPerformed(
            UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            /*
             * This is the transition between the "waking up" state and
             * the normal gameplay state.
             *
             * Several systems must be synchronized here:
             *
             * 1. Fade out the black screen.
             * 2. Hide the wake-up instruction.
             * 3. Give control back to the normal gameplay camera.
             * 4. Stop the pager sound.
             * 5. Start the captain's introduction dialogue.
             * 6. Resume the boat.
             * 7. Remove this input callback because the player only needs
             *    to wake up once.
             *
             * The captain dialogue is queued rather than played directly,
             * allowing the TalkieManager to handle it through its normal
             * dialogue system.
             */

            // Reveal the gameplay scene.
            BlackScreenController.Current.StartFade(0f, 3f);

            // Hide the wake-up instruction.
            BlackScreenController.Current.FadeWakeUpText(0f, 0.5f);

            // Return control to the normal gameplay camera.
            _context.WakeUpCam.Priority = -1;

            // Stop the pager sound.
            _context.TalkieManager.StopBip();

            // Start the captain's introduction.
            _context.TalkieManager.Enqueue(_captainInitialDialogue);

            // The wake-up input is only required once.
            InputManager.PIA.Player.Jump.performed -= JumpPerformed;
        }

        #endregion


        #region CAPTAIN DIALOGUES

        private void TalkieManager_OnDialogueFinished(
            LocalizedDialogueAudio dialogue)
        {
            /*
             * The tutorial progression is driven by dialogue completion.
             *
             * We intentionally do not use timers here. This guarantees that
             * the next tutorial action only becomes available after the
             * corresponding audio has actually finished playing.
             */

            if (dialogue == _captainInitialDialogue)
            {
                HandleInitialDialogueFinished();
            }
            else if (dialogue == _captainReminderToMoveDialogue)
            {
                HandleReminderDialogueFinished();
            }
        }


        private void HandleInitialDialogueFinished()
        {
            /*
             * The captain has finished explaining the initial situation.
             * The player can now start moving.
             *
             * This method marks the beginning of the movement objective:
             *
             *     Dialogue finished
             *          ↓
             *     Unlock player
             *          ↓
             *     Set movement objective
             *          ↓
             *     Start inactivity timer
             *          ↓
             *     Listen for movement input
             *
             * The timer is not started before this point because the player
             * should not be penalized for being inactive while the captain
             * is still speaking.
             */

            UnlockPlayerInputs();

            ObjectiveManager.Current.SetObjective("Move with ZQSD");

            _isPlayerHasToMove = true;

            StartMovementTimer();

            InputManager.PIA.Player.Move.performed += MovePerformed;
        }


        private void HandleReminderDialogueFinished()
        {
            /*
             * Once the reminder dialogue has finished, the player gets
             * another full inactivity period before another reminder can
             * be triggered.
             */

            _timerWhenPlayerNotMoving.StartTimer();
        }

        #endregion


        #region PLAYER MOVEMENT

        private void StartMovementTimer()
        {
            /*
             * The timer measures how long the player remains inactive after
             * receiving the movement objective.
             *
             * The completion callback is subscribed here so the timer can
             * trigger the captain's reminder dialogue automatically.
             */

            _timerWhenPlayerNotMoving.StartTimer();

            _timerWhenPlayerNotMoving.OnTimerComplete += TimerWhenPlayerNotMoving_OnTimerComplete;
        }


        private void MovePerformed(
            UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            /*
             * The objective is completed as soon as the player performs
             * any movement input.
             *
             * We remove the callback immediately because movement is only
             * required once for this tutorial step. This also prevents
             * subsequent movement inputs from repeatedly executing the
             * completion logic.
             */

            InputManager.PIA.Player.Move.performed -= MovePerformed;

            ObjectiveManager.Current.CompleteObjective();

            IsComplete = true;
        }


        private void TimerWhenPlayerNotMoving_OnTimerComplete()
        {
            /*
             * If the player has not moved for the configured amount of time,
             * the captain reminds them of the current objective.
             *
             * The timer is reset immediately so that another reminder can
             * happen after the same delay if the player continues to remain
             * inactive.
             *
             * The actual restart of the timer after the dialogue is also
             * handled in HandleReminderDialogueFinished(), which means the
             * player receives a complete inactivity period after the reminder
             * has finished playing.
             */

            _context.TalkieManager.Enqueue(_captainReminderToMoveDialogue);

            _timerWhenPlayerNotMoving.ResetTimer();
        }

        #endregion


        #region PLAYER INPUT

        private void UnlockPlayerInputs()
        {
            /*
             * The player starts the tutorial with gameplay inputs restricted.
             *
             * Inventory and interaction inputs are explicitly enabled here,
             * while EnableAllCharacterInputs restores the normal character
             * controls.
             *
             * Camera rotation is enabled independently because camera control
             * is intentionally kept available throughout the tutorial.
             */

            if (PlayerHandlerData.MainPlayer == null)
                return;

            PlayerHandlerData.MainPlayer.Inventory.Enable();
            PlayerHandlerData.MainPlayer.Interactions.Enable();

            PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = true;
            PlayerHandlerData.MainPlayer.EnableCameraRotationInput = true;
        }

        #endregion


        #region COROUTINES

        private IEnumerator WaitForPlayerInputRoutine(
            WaitForSeconds delay,
            Action onEnd)
        {
            /*
             * Generic helper used by the wake-up sequence.
             *
             * Instead of creating several nearly identical coroutine methods,
             * this method receives:
             *
             * - the amount of time to wait;
             * - the callback to execute afterwards.
             *
             * This allows us to express sequences such as:
             *
             *     Wait 4 seconds → OnPagerDelayEnded()
             *
             * or:
             *
             *     Wait 5 seconds → OnFirstDelayEnded()
             *
             * without duplicating coroutine code.
             */

            yield return delay;

            onEnd?.Invoke();
        }

        #endregion
    }
}