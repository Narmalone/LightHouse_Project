using LightHouse.Audio;
using UnityEngine;

namespace LightHouse.Game.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepUseHammer")]
    public sealed class StepUseHammer : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial7;

        private TutorialContext _ctx;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _tutorial7.Register();
            _ctx.NearbyBuoy.BreakDown();
        }

        public override void Tick(TutorialContext ctx, float dt)
        {
            if (IsComplete) return;
        }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial7.Unregister();
        }
    }
}
