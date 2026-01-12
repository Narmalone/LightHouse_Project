using LightHouse.Core.Player;
using LightHouse.Core.Player.Inventory;
using LightHouse.Core.Audio;
using LightHouse.Features.Items.Inventory;
using LightHouse.Features.Items.Other;

using System.Collections;
using UnityEngine;

namespace LightHouse.Core.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/IntroSequence")]
    public sealed class StepIntroSequence : TutorialStep
    {
        [SerializeField] private float _delayBeforeBip = 7.5f;
        [SerializeField] private float _delayBeforeDialogues = 7.5f;
        [SerializeField] private LayerMask _physicsMoverMask;

        [SerializeField] private LocalizedDialogueAudio _t1;
        [SerializeField] private LocalizedDialogueAudio _t2;

        private MonoBehaviour _runner;

        private TutorialContext _ctx;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);

            InitializeControllersForTheFirstStep(ctx);
            // Register ici OU dans un bootstrap global (à toi de voir)
            _t1.Register();
            _t2.Register();
            ctx.TalkieManager.OnDialogueFinished += OnDialogueFinished;
            _runner = ctx.Flow; // simple; sinon injecte le runner dans ctx
            _ctx = ctx;
            _runner.StartCoroutine(Routine(ctx));

            if (PlayerHandlerData.MainPlayer != null)
            {
                PlayerHandlerData.MainPlayer.Inventory.Disable();
                PlayerHandlerData.MainPlayer.Interactions.Disable();
                PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = false;

                PlayerHandlerData.MainPlayer.EnableCameraRotationInput = false;
            }
        }

        private void OnDialogueFinished(LocalizedDialogueAudio audio)
        {
            if(audio == _t2)
            {
                // fin du step après le second dialogue
                _ctx.TalkieManager.OnDialogueFinished -= OnDialogueFinished;
                IsComplete = true;
            }
        }

        private IEnumerator Routine(TutorialContext ctx)
        {
            if (_delayBeforeBip > 0f) yield return new WaitForSeconds(_delayBeforeBip);
            ctx.TalkieManager?.Bip();

            // spawn item (comme toi)
            IInventoryItem generatedItem =
                PlayerHandlerData.MainPlayer.Inventory.GenerateAndAddItemToInventory(SlotManager.CurrentSlotIndex, 5, false);

            generatedItem.GetGameObject()
                .AddComponent<MoverFollower>()
                .Config(_physicsMoverMask, 0.2f);

            if (_delayBeforeDialogues > 0f) yield return new WaitForSeconds(_delayBeforeDialogues);
            ctx.TalkieManager?.StopBip();

            ctx.Talkie?.Enqueue(_t1);
            ctx.Talkie?.Enqueue(_t2);
            if (PlayerHandlerData.MainPlayer != null)
            {
                PlayerHandlerData.MainPlayer.Inventory.Enable();
                PlayerHandlerData.MainPlayer.Interactions.Enable();
                //PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = true;
            }
        }

        public override void Exit(TutorialContext ctx)
        {
            _t1.Unregister();
            _t2.Unregister();
        }

        private void InitializeControllersForTheFirstStep(TutorialContext ctx)
        {
            ctx.NearbyBuoy?.BreakDown();
            ctx.Bag?.gameObject.SetActive(false);
        }
    }
}
