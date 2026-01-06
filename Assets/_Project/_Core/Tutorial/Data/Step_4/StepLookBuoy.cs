using LightHouse.Audio;
using UnityEngine;

namespace LightHouse.Game.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/StepLookBuoy")]
    public sealed class StepLookBuoy : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial6;

        [SerializeField, Range(0.75f, 0.999f)]
        private float _lookDotThreshold = 0.93f;

        [SerializeField, Min(0f)]
        private float _maxDistanceToValidate = 0f;

        private TutorialContext _ctx;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _tutorial6.Register();
            //_ctx.NearbyBuoy.BreakDown();
        }

        public override void Tick(TutorialContext ctx, float dt)
        {
            if (IsComplete) return;

            if (ctx.NearbyBuoy == null)
                return;

            Transform camT = ctx.ViewTransform;
            if (camT == null) return;

            Vector3 to = ctx.NearbyBuoy.transform.position - camT.position;

            if (_maxDistanceToValidate > 0f && to.sqrMagnitude > _maxDistanceToValidate * _maxDistanceToValidate)
                return;

            to.Normalize();
            float dot = Vector3.Dot(camT.forward, to);

            if (dot >= _lookDotThreshold)
            {
                ctx.Talkie?.Enqueue(_tutorial6);
                IsComplete = true;
            }
        }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial6.Unregister();
        }
    }
}
