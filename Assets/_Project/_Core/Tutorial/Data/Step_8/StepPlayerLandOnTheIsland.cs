using LightHouse.Audio;
using LightHouse.Weather;
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

            ChangeCurrentWeatherToStorm();

            _tutorialBoat.BoatPathMover.OnPathCompleted -= BoatMover_OnPathCompleted;
        }

        private void ChangeCurrentWeatherToStorm()
        {
            if (_ctx.Timeline == null) return;

            int idx = _ctx.Timeline.CurrentIndex;
            if (idx < 0 || idx + 1 >= _ctx.Timeline.Weathers.Count) return;

            var w0 = _ctx.Timeline.GenerateSingleWeatherData(
                WeatherType.Stormy,
                _ctx.Timeline.Weathers[idx].StartTimeInSeconds,
                _ctx.Timeline.Weathers[idx].DurationInSeconds
            );

            var w1 = _ctx.Timeline.GenerateSingleWeatherData(
                WeatherType.Stormy,
                _ctx.Timeline.Weathers[idx + 1].StartTimeInSeconds,
                _ctx.Timeline.Weathers[idx + 1].DurationInSeconds
            );

            _ctx.Timeline.Weathers[idx] = w0;
            _ctx.Timeline.Weathers[idx + 1] = w1;
            _ctx.Timeline.NotifyChanged();
        }

        public override void Tick(TutorialContext ctx, float dt) { }

        public override void Exit(TutorialContext ctx)
        {
            _tutorial11.Unregister();
        }
    }
}
