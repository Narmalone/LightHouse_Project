using System.Collections;
using UnityEngine;
using LightHouse.Inventory;
using LightHouse.Handlers;
using LightHouse.Audio;
using System;

namespace LightHouse.Game.Tutorial.Steps
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
        }
    }
}
