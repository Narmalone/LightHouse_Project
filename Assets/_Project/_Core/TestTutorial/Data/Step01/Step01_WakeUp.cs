using UnityEngine;
using System.Collections;
using LightHouse.Features.Tutorial;
using LightHouse.Core.Tutorial;
using System;
using LightHouse.Core.Inputs;
using Cinemachine;
using LightHouse.Core.Audio;
using LightHouse.Features.Talkie;
using UnityEngine.Localization;
using LightHouse.Core.Localization;

[CreateAssetMenu(fileName = "Step01_WakeUp", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step01_WakeUp")]
public class Step01_WakeUp : TutorialStep
{
    private MonoBehaviour _routineBehaviour;
    private WaitForSeconds _delayPlayerWakeAfterPagerBip;
    private CinemachineVirtualCamera _wakeUpCam;
    private TalkieManager _talkieManager;
    private string _wakeUpInteractionText;
    [SerializeField] private LocalizedString _wakeUpText;
    [SerializeField] private LocalizedString _pressToAction;
    [SerializeField] private LocalizedDialogueAudio _captainInitialDialogue;
    [SerializeField] private float _delayBeforePlayerCanInputDuration = 5f;

    public async override void Enter(TutorialContext context)
    {
        _talkieManager = context.TalkieManager;
        _routineBehaviour = context.Flow;
        _delayPlayerWakeAfterPagerBip = new WaitForSeconds(_delayBeforePlayerCanInputDuration);

        //black screen & wake up camera priority
        BlackScreenController.Current.StartFade(1f, -1f);
        _wakeUpCam = context.WakeUpCam;
        _wakeUpCam.Priority = 1000;

        //play sound of waves

        //wait qlq 4 secondes

        _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine(new WaitForSeconds(4f), () =>
        {
            _talkieManager.Bip();
            _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine(_delayPlayerWakeAfterPagerBip, OnFirstDelayEnded));
        }));


        //play sound of walkie

        //wait qlq 5 secondes
        

        //captain dialogue

        //wait

        //tutorial: space bar to get up

        //open eyes & wake up camera priority
        //camera transition to player

        //next step
    }

    private async void OnFirstDelayEnded()
    {
        string inputName = InputManager.Jump_Bind_Name;
        _wakeUpInteractionText = await InteractionTextBuilder.Build_Hold_To_Action(_wakeUpText, inputName, _pressToAction);
        BlackScreenController.Current.SetWakeUpText(_wakeUpInteractionText);

        InputManager.Jump.performed += JumpPerformed;
        BlackScreenController.Current.FadeWakeUpText(1f, 2f, null, null);
    }

    private async void JumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log("Player Performed");
        BlackScreenController.Current.StartFade(0f, 3f);
        BlackScreenController.Current.FadeWakeUpText(0f, 0.5f);
        _wakeUpCam.Priority = -1;
        _talkieManager.StopBip();
        InputManager.Jump.performed -= JumpPerformed;
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
