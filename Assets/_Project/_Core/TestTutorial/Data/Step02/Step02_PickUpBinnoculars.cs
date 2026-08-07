using LightHouse.Core.Audio;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step02_PickUpBinnoculars", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step02_PickUpBinnoculars")]

    public class Step02_PickUpBinnoculars : TutorialStep
    {
        #region REFERENCES

        private TutorialContext _context;

        #endregion


        #region CONFIGURATION

        [Header("Captain Dialogues")]

        [SerializeField] private LocalizedDialogueAudio _captainInitialDialogue;

        [SerializeField] private LocalizedDialogueAudio _captainPickUpBinnocularsDialogue;

        [SerializeField] private LocalizedDialogueAudio _captainLeadWayDialogue;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            /*
             * This tutorial step is driven by two types of events:
             *
             * - Dialogue events control the progression of the tutorial.
             * - Binocular events detect when the player picks up or uses
             *   the binoculars.
             *
             * The complete flow is:
             *
             *     Enter
             *       ↓
             *     Initial dialogue
             *       ↓
             *     Pick-up dialogue
             *       ↓
             *     Player can pick up binoculars
             *       ↓
             *     Binoculars picked up
             *       ↓
             *     Lead-way dialogue
             *       ↓
             *     Player goes to the front
             *       ↓
             *     Player uses binoculars
             *       ↓
             *     Tutorial step completed
             *
             * We subscribe to all required events before starting the
             * dialogue sequence so that no event can be missed.
             */

            SubscribeToEvents();
            StartTutorialSequence();
        }


        public override void Exit(TutorialContext context)
        {
            /*
             * Always remove event subscriptions when leaving the step.
             *
             * Tutorial steps can be entered and exited multiple times.
             * Keeping an old subscription would cause this step to react
             * to events even though it is no longer active.
             */

            UnsubscribeFromEvents();
        }

        #endregion


        #region INITIALIZATION

        private void StartTutorialSequence()
        {
            /*
             * The first two dialogues are queued immediately.
             *
             * The TalkieManager controls when each dialogue actually plays.
             * Because the second dialogue is queued after the first one,
             * we don't need to manually wait for the first dialogue to end.
             *
             * The OnDialogueFinished event will tell us when the second
             * dialogue has finished so we can unlock the binoculars.
             */

            _context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

            // Introduce the current tutorial situation.
            _context.TalkieManager.Enqueue(_captainInitialDialogue);

            // Explain that the binoculars need to be picked up.
            _context.TalkieManager.Enqueue(_captainPickUpBinnocularsDialogue);
        }

        #endregion


        #region EVENTS

        private void SubscribeToEvents()
        {
            /*
             * These events are related directly to the player's interaction
             * with the binoculars.
             *
             * ItemAddedToInventory:
             *     Triggered when the binoculars have actually been picked up.
             *
             * OnItemUsed:
             *     Triggered when the player uses the binoculars.
             */

            _context.Binocular.ItemAddedToInventory += OnBinocularPickedUp;
            _context.Binocular.OnItemUsed += OnBinocularUsed;
        }


        private void UnsubscribeFromEvents()
        {
            /*
             * Remove every event subscription created by this tutorial step.
             *
             * This prevents callbacks from being triggered after the step
             * has been completed or exited.
             */

            _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            _context.Binocular.ItemAddedToInventory -= OnBinocularPickedUp;
            _context.Binocular.OnItemUsed -= OnBinocularUsed;
        }

        #endregion


        #region DIALOGUES

        private void OnDialogueFinished(LocalizedDialogueAudio dialogue)
        {
            /*
             * Dialogue completion is used as a synchronization point for
             * the tutorial.
             *
             * We compare the finished dialogue with the references configured
             * in the Inspector to determine which part of the tutorial should
             * be unlocked next.
             *
             * Pick-up dialogue finished:
             *     → Make binoculars pickable.
             *     → Tell player to pick them up.
             *
             * Lead-way dialogue finished:
             *     → Tell player to go to the front.
             *     → Wait for binocular usage.
             */

            if (dialogue == _captainPickUpBinnocularsDialogue)
            {
                HandlePickUpDialogueFinished();
            }
            else if (dialogue == _captainLeadWayDialogue)
            {
                HandleLeadWayDialogueFinished();
            }
        }


        private void HandlePickUpDialogueFinished()
        {
            /*
             * The captain has finished explaining that the binoculars
             * need to be picked up.
             *
             * Only now do we make the binoculars available to the player.
             *
             * This ensures that the player cannot complete this part of
             * the tutorial before the corresponding dialogue has finished.
             */

            ObjectiveManager.Current.SetObjective("Pick up the binoculars");

            _context.Binocular.SetPickable(true);
        }


        private void HandleLeadWayDialogueFinished()
        {
            /*
             * The player has already picked up the binoculars and the
             * captain has finished explaining the next step.
             *
             * The objective now changes from "pick up" to "go to the front
             * and use the binoculars".
             *
             * No input callback is required here because the binocular
             * component itself is responsible for notifying us when the
             * player actually uses the item.
             */

            ObjectiveManager.Current.SetObjective("Get at the front and use the binoculars with left click");
        }

        #endregion


        #region BINOCULAR

        private void OnBinocularPickedUp()
        {
            /*
             * This callback means that the player has successfully completed
             * the first binocular objective.
             *
             * The current objective is completed immediately, then the
             * captain's next dialogue is queued.
             *
             * We don't directly set the next objective here because the
             * dialogue should play first. The objective will be updated by
             * HandleLeadWayDialogueFinished() once that dialogue has ended.
             *
             * The progression is therefore:
             *
             *     Pick up binoculars
             *            ↓
             *     Complete objective
             *            ↓
             *     Play captain dialogue
             *            ↓
             *     Set next objective
             */

            _context.TalkieManager.Enqueue(_captainLeadWayDialogue);

            ObjectiveManager.Current.CompleteObjective();
        }


        private void OnBinocularUsed()
        {
            /*
             * At this point the player has reached the final action required
             * by this tutorial step.
             *
             * We lock the player's character inputs before completing the
             * objective so the player cannot continue performing tutorial
             * interactions while the next tutorial step is being prepared.
             *
             * IsComplete is set to true only after the objective has been
             * completed, which signals to the Tutorial system that this step
             * can now be exited.
             */

            ObjectiveManager.Current.CompleteObjective();

            IsComplete = true;
        }

        #endregion
    }
}