using LightHouse.Audio;
using System;
using UnityEngine;

namespace LightHouse.Game.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepPlayerLandOnTheIsland")]
    public sealed class StepPlayerLandOnTheIsland : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial11;

        private TutorialBoat _tutorialBoat;
        private TutorialContext _ctx;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _tutorialBoat = ctx.TutoBoat;
            _tutorial11.Register();
            _tutorialBoat.BoatPathMover.OnPathCompleted += BoatMover_OnPathCompleted;
        }

        private void BoatMover_OnPathCompleted()
        {
            _ctx.Talkie?.Enqueue(_tutorial11);
            foreach(Collider collider in _tutorialBoat.SubBoatColliders)
            {
                collider.enabled = false;
            }
            _tutorialBoat.BoatPathMover.OnPathCompleted -= BoatMover_OnPathCompleted;
        }

        public override void Tick(TutorialContext ctx, float dt) { }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial11.Unregister();
        }
    }
}
