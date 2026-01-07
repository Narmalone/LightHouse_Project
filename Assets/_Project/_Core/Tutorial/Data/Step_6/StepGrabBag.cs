using LightHouse.Audio;
using UnityEngine;

namespace LightHouse.Game.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepUseHammer")]
    public sealed class StepGrabBag : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial9;

        private TutorialContext _ctx;

        //
        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _tutorial9.Register();
            _ctx.Talkie?.Enqueue(_tutorial9);
        }

        public override void Tick(TutorialContext ctx, float dt)
        {
            if (IsComplete) return;
        }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial9.Unregister();
        }
    }
}
