using LightHouse.Core.Tutorial;
using LightHouse.Features.Boats;
using UnityEngine;

public class StepSkipForBoat : TutorialStep
{
    [SerializeField] private float _boatSpeedDuringSkip = 25.0f;

    private BoatPathMover pathMover;
    public override void Enter(TutorialContext ctx)
    {
        StartSkipBoat(ctx);
    }
    public void StartSkipBoat(TutorialContext ctx)
    {
        pathMover = ctx.TutoBoat.BoatPathMover;
        pathMover.OnPathCompleted += PathMover_OnPathCompleted;
        pathMover.Speed = _boatSpeedDuringSkip;
    }

    private void PathMover_OnPathCompleted()
    {
        pathMover.Speed = pathMover.BaseMoveSpeed;
        IsComplete = true;
    }

    public override void Exit(TutorialContext ctx)
    {
        pathMover = ctx.TutoBoat.BoatPathMover;
        pathMover.OnPathCompleted -= PathMover_OnPathCompleted;
    }
}
