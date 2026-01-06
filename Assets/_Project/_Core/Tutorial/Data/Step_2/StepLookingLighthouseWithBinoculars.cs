using LightHouse.Audio;
using UnityEngine;

namespace LightHouse.Game.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "LightHouse/Tutorial/Steps/LookLighthouseWithBinoculars")]
    public sealed class StepLookLighthouseWithBinoculars : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _tutorial3;
        [SerializeField] private LocalizedDialogueAudio _tutorial4;

        [SerializeField, Range(0.75f, 0.999f)]
        private float _lookDotThreshold = 0.93f;

        [SerializeField, Min(0f)]
        private float _maxDistanceToValidate = 0f;

        private TutorialContext _ctx;

        public override void Enter(TutorialContext ctx)
        {
            base.Enter(ctx);
            _ctx = ctx;
            _ctx.Binocular.ItemAddedToInventory += Binocular_ItemAddedToInventory;
            _ctx.Binocular.CanBeRaycasted = true;
            _tutorial3.Register();
            _tutorial4.Register();
        }

        private void Binocular_ItemAddedToInventory()
        {
            _ctx.Talkie?.Enqueue(_tutorial3);
            _ctx.Binocular.ItemAddedToInventory -= Binocular_ItemAddedToInventory;
        }

        public override void Tick(TutorialContext ctx, float dt)
        {
            if (IsComplete) return;

            if (ctx.Binocular == null || !ctx.Binocular.IsBinocularModeUsed)
                return;

            if (ctx.Lighthouse == null)
                return;

            Transform camT = ctx.ViewTransform;
            if (camT == null) return;

            Vector3 to = ctx.Lighthouse.position - camT.position;

            if (_maxDistanceToValidate > 0f && to.sqrMagnitude > _maxDistanceToValidate * _maxDistanceToValidate)
                return;

            to.Normalize();
            float dot = Vector3.Dot(camT.forward, to);

            if (dot >= _lookDotThreshold)
            {
                ctx.Talkie?.Enqueue(_tutorial4);
                IsComplete = true;
            }
        }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial3.Unregister();
            _tutorial4.Unregister();
        }
    }
}
