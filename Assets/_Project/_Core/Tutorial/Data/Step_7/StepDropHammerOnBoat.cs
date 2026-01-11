using LightHouse.Core.Audio;
using LightHouse.Features.Items.Other;
using UnityEngine;

namespace LightHouse.Core.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepDropHammerOnTheBoat")]
    public sealed class StepDropHammerOnTheBoat : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial10;

        private TutorialContext _ctx;

        private MoverFollower _hammerFolower;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _hammerFolower = _ctx.Hammer.GetComponent<MoverFollower>();
            _hammerFolower.OnMoverAttached += Hammer_OnMoverAttached;
            _tutorial10.Register();
            _ctx.Talkie?.Enqueue(_tutorial10);
        }

        private void Hammer_OnMoverAttached()
        {
            IsComplete = true;
            _hammerFolower.OnMoverAttached -= Hammer_OnMoverAttached;
        }

        public override void Tick(TutorialContext ctx, float dt)
        {
            if (IsComplete) return;
        }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial10.Unregister();
        }
    }
}
