using LightHouse.Audio;
using UnityEngine;

namespace LightHouse.Game.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepUseHammer")]
    public sealed class StepUseHammer : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial7;
        [SerializeField] private LocalizedDialogueAudio _tutorial8;

        private TutorialContext _ctx;

        //
        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _tutorial7.Register();
            _tutorial8.Register();
            _ctx.Talkie?.Enqueue(_tutorial7);
            _ctx.Hammer.gameObject.SetActive(true);
            _ctx.Pipe.OnItemUsedOnMe += OnPipeUsed;
            _ctx.Pipe.CanBeRaycasted = true;
            _ctx.Pipe.CanBeInteracted = true;
        }

        private void OnPipeUsed()
        {
            //TO DOO Remplacer le pipe fantome par le vrai pipe.
            _ctx.Talkie?.Enqueue(_tutorial8);
            _ctx.Pipe.OnItemUsedOnMe -= OnPipeUsed;
            IsComplete = true;
        }

        public override void Tick(TutorialContext ctx, float dt) { }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial7.Unregister();
            _tutorial8.Unregister();
        }
    }
}
