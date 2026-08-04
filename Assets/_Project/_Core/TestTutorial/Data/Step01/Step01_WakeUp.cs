using UnityEngine;
using System.Collections;
using LightHouse.Features.Tutorial;
using LightHouse.Core.Tutorial;
using System;

public class Step01_WakeUp : TutorialStep
{
    private MonoBehaviour _routineBehaviour;
    private WaitForSeconds _delayBeforePlayerCanInput;
    [SerializeField] private float _delayBeforePlayerCanInputDuration = 5f;

    public override void Enter(TutorialContext context)
    {
        _routineBehaviour = context.Flow;
        _delayBeforePlayerCanInput = new WaitForSeconds(_delayBeforePlayerCanInputDuration);

        //black screen & wake up camera priority
        BlackScreenController.Current.StartFade(1f, 0f);
        context.WakeUpCam.Priority = 10;

        //play sound of waves

        //wait qlq 10 15secondes
        _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine(_delayBeforePlayerCanInput, () => 
        { 
        
        }));

        //play sound of walkie

        //wait qlq 5 secondes

        //captain dialogue

        //wait

        //tutorial: space bar to get up

        //open eyes & wake up camera priority
        BlackScreenController.Current.StartFade(0f, 3f);
        context.WakeUpCam.Priority = 0;
        //camera transition to player

        //next step
    }

    public override void Exit(TutorialContext context)
    {

    }

    private IEnumerator WaitForPlayerInputRoutine(WaitForSeconds delay, Action onEnd)
    {
        yield return delay;
        onEnd?.Invoke();
    }
}
