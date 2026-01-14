using LightHouse.Core.Audio;
using UnityEngine;

namespace LightHouse.Core.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepGrabBag")]
    public sealed class StepGrabBag : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial9;

        private TutorialContext _ctx;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _ctx.Bag.OnObjectInteracted += Bag_OnObjectInteracted;
            ctx.Bag.CanBeInteracted = true;
            ctx.Bag.InvokeInteractionDescriptionUpdated();
            _tutorial9.Register();
            _ctx.Talkie?.Enqueue(_tutorial9);
        }

        private void Bag_OnObjectInteracted()
        {
            IsComplete = true;
            _ctx.Bag.OnObjectInteracted -= Bag_OnObjectInteracted;
        }

        public override void Tick(TutorialContext ctx, float dt) { }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial9.Unregister();
        }
    }
}
