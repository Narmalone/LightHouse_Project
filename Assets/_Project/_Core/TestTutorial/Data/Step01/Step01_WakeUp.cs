using Cinemachine;
using LightHouse.Core.Audio;
using LightHouse.Core.Inputs;
using LightHouse.Core.Localization;
using LightHouse.Core.Player;
using LightHouse.Core.Tutorial;
using LightHouse.Core.Utilities;
using LightHouse.Features.Talkie;
using LightHouse.Features.Tutorial;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Step01_WakeUp", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step01_WakeUp")]
public class Step01_WakeUp : TutorialStep
{
    private MonoBehaviour _routineBehaviour;
    private WaitForSeconds _delayPlayerWakeAfterPagerBip;

    private TutorialContext _context;
    private string _wakeUpInteractionText;
    private Timer _timerWhenPlayerNotMoving;
    private bool _isPlayerHasToMove = false;
    [SerializeField] private float _timeWhenPlayerNotMoving = 10f;
    [SerializeField] private LocalizedString _wakeUpText;
    [SerializeField] private LocalizedString _pressToAction;
    [SerializeField] private LocalizedDialogueAudio _captainInitialDialogue;
    [SerializeField] private LocalizedDialogueAudio _captainReminderToMoveDialogue;
    [SerializeField] private float _delayBeforePlayerCanInputDuration = 5f;

    public override void Enter(TutorialContext context)
    {
        base.Enter(context);
        _context = context;
        _routineBehaviour = context.Flow;
        _delayPlayerWakeAfterPagerBip = new WaitForSeconds(_delayBeforePlayerCanInputDuration);

        //black screen & wake up camera priority
        BlackScreenController.Current.StartFade(1f, -1f);
        context.WakeUpCam.Priority = 1000;

        _isPlayerHasToMove = false;
        _timerWhenPlayerNotMoving = new Timer(_timeWhenPlayerNotMoving);

        context.TalkieManager.OnDialogueFinished += TalkieManager_OnDialogueFinished;

        _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine(new WaitForSeconds(4f), () =>
        {
            context.TalkieManager.Bip();
            _routineBehaviour.StartCoroutine(WaitForPlayerInputRoutine(_delayPlayerWakeAfterPagerBip, OnFirstDelayEnded));
        }));

        context.TutoBoat.Pause();
    }

    public override void Tick(TutorialContext context, float dt)
    {
        if (!_isPlayerHasToMove) return;

        _timerWhenPlayerNotMoving.Tick(dt);
    }

    private void TalkieManager_OnDialogueFinished(LocalizedDialogueAudio obj)
    {
        if (obj == _captainInitialDialogue)
        {
            UnlockPlayerInputs();
            ObjectiveManager.Current.SetObjective("Move with ZQSD");
            _isPlayerHasToMove = true;
            _timerWhenPlayerNotMoving.StartTimer();
            _timerWhenPlayerNotMoving.OnTimerComplete += TimerWhenPlayerNotMoving_OnTimerComplete;
            InputManager.Player.Move.performed += MovePerformed;
        }
        else if(obj == _captainReminderToMoveDialogue)
        {
            _timerWhenPlayerNotMoving.StartTimer();
        }
    }

    private void TimerWhenPlayerNotMoving_OnTimerComplete()
    {
        _context.TalkieManager.Enqueue(_captainReminderToMoveDialogue);
        _timerWhenPlayerNotMoving.ResetTimer();
    }

    private void MovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        InputManager.Player.Move.performed -= MovePerformed;
        ObjectiveManager.Current.CompleteObjective();
        IsComplete = true;
    }

    private void UnlockPlayerInputs()
    {
        if (PlayerHandlerData.MainPlayer != null)
        {
            PlayerHandlerData.MainPlayer.Inventory.Enable();
            PlayerHandlerData.MainPlayer.Interactions.Enable();
            PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = true;
            PlayerHandlerData.MainPlayer.EnableCameraRotationInput = true;
        }
    }

    private async void OnFirstDelayEnded()
    {
        string inputName = InputManager.Jump_Bind_Name;
        _wakeUpInteractionText = await InteractionTextBuilder.Build_Hold_To_Action(_wakeUpText, inputName, _pressToAction);
        BlackScreenController.Current.SetWakeUpText(_wakeUpInteractionText);

        InputManager.Jump.performed += JumpPerformed;
        BlackScreenController.Current.FadeWakeUpText(1f, 2f, null, null);
    }

    private void JumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        BlackScreenController.Current.StartFade(0f, 3f);
        BlackScreenController.Current.FadeWakeUpText(0f, 0.5f);
        _context.WakeUpCam.Priority = -1;
        _context.TalkieManager.StopBip();
        _context.TalkieManager.Enqueue(_captainInitialDialogue);

        _context.TutoBoat.Resume();
        InputManager.Jump.performed -= JumpPerformed;
    }

    public override void Exit(TutorialContext context)
    {
        context.TalkieManager.OnDialogueFinished -= TalkieManager_OnDialogueFinished;
        if (_timerWhenPlayerNotMoving != null)
        {
            _timerWhenPlayerNotMoving.OnTimerComplete -= TimerWhenPlayerNotMoving_OnTimerComplete;
        }
    }

    private IEnumerator WaitForPlayerInputRoutine(WaitForSeconds delay, Action onEnd)
    {
        yield return delay;
        onEnd?.Invoke();
    }
}