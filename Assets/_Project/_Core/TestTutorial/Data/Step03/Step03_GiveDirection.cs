using LightHouse.Core.Audio;
using LightHouse.Core.Player;
using LightHouse.Core.Tutorial;
using LightHouse.Core.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "Step03_GiveDirection", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step03_GiveDirection")]
public class Step03_GiveDirection : TutorialStep
{
    private Timer _forcedChoice;
    [SerializeField] private float _timerChoiceDuration = 10;
    private TutorialContext _context;
    private bool _tickActivated;
    [SerializeField] private LocalizedDialogueAudio _choiceIntroduction; //empty text to start the choices
    [SerializeField] private LocalizedDialogueAudio _captainBadWayGuide; //première erreur de direction
    [SerializeField] private LocalizedDialogueAudio _captainBadWay; //si une erreur de direction est faite
    [SerializeField] private LocalizedDialogueAudio _captainBadWayHalfFalse; //si pas erreur au premier et erreur au deuxième choix
    [SerializeField] private LocalizedDialogueAudio _captainBadWayRepairEngine; //répare le moteur
    [SerializeField] private LocalizedDialogueAudio _captainBadWayEndSubTutorial; //retire argent du salaire
    [SerializeField] private LocalizedDialogueAudio _captainGoodWayGuide; //première bonne direction
    [SerializeField] private LocalizedDialogueAudio _captainGoodWay; //deuxième bonne direction

    public override void Enter(TutorialContext context)
    {
        base.Enter(context);

        _context = context;

        ObjectiveManager.Current.SetObjective("Give the captain directions");

        _forcedChoice = new Timer(_timerChoiceDuration);

        context.TalkieManager.Enqueue(_choiceIntroduction);

        _choiceIntroduction.OnChoiceSelected += Captain_OnFirstChoiceSelected;
        context.TalkieManager.OnDialogueFinished += OnDialogueFinished;
        _forcedChoice.OnTimerComplete += TimerChoiceDuration_OnTimerComplete;

    }

    public override void Tick(TutorialContext context, float dt)
    {
        if (_tickActivated)
        {
            _forcedChoice.Tick(dt);
        }
    }

    private void OnDialogueFinished(LocalizedDialogueAudio obj)
    {
        if (obj == _captainBadWayEndSubTutorial)
        {
            IsComplete = true;
        }

        else if (obj == _choiceIntroduction)
        {
            _tickActivated = true;
            _forcedChoice.StartTimer();
        }

        else if (obj == _captainGoodWay)
        {
            ObjectiveManager.Current.CompleteObjective();
            LockPlayerInputs();
            IsComplete = true;
        }

        else if (obj == _captainBadWay)
        {
            ObjectiveManager.Current.CompleteObjective();
            LockPlayerInputs();
            _context.TalkieManager.Enqueue(_captainBadWayRepairEngine);
            _context.TalkieManager.Enqueue(_captainBadWayEndSubTutorial);
            //marteau unlockable
        }

        else if (obj == _captainBadWayHalfFalse)
        {
            ObjectiveManager.Current.CompleteObjective();
            LockPlayerInputs();
            _context.TalkieManager.Enqueue(_captainBadWayRepairEngine);
            _context.TalkieManager.Enqueue(_captainBadWayEndSubTutorial);
            //marteau unlockable
        }
    }

    private void TimerChoiceDuration_OnTimerComplete()
    {
        _context.TalkieManager.ForceChoice(1);
    }

    private void Captain_OnFirstChoiceSelected(TalkieChoice obj)
    {
        _tickActivated = false;
        _forcedChoice.ResetTimer(false);
        _forcedChoice.StopTimer();

        if (obj.Index == 2)
        {
            _context.TalkieManager.Enqueue(_captainGoodWayGuide);
            _captainGoodWayGuide.OnChoiceSelected += CaptainGoodWayGuide_OnChoiceSelected;

            _tickActivated = true;
            _forcedChoice.StartTimer();
        }

        else
        {
            _context.TalkieManager.Enqueue(_captainBadWayGuide);
            _captainBadWayGuide.OnChoiceSelected += CaptainBadWayGuide_OnChoiceSelected;

            _tickActivated = true;
            _forcedChoice.StartTimer();
        }
    }

    private void CaptainBadWayGuide_OnChoiceSelected(TalkieChoice obj)
    {
        _tickActivated = false;
        _forcedChoice.ResetTimer(false);
        _forcedChoice.StopTimer();

        if (obj.Index == 1)
        {
            _context.TalkieManager.Enqueue(_captainBadWayHalfFalse);
        }

        else
        {
            _context.TalkieManager.Enqueue(_captainBadWay);
        }
    }

    private void CaptainGoodWayGuide_OnChoiceSelected(TalkieChoice obj)
    {
        _tickActivated = false;
        _forcedChoice.ResetTimer(false);
        _forcedChoice.StopTimer();

        if (obj.Index == 1)
        {
            _context.TalkieManager.Enqueue(_captainGoodWay);
        }

        else
        {
            _context.TalkieManager.Enqueue(_captainBadWay);
        }
    }
    private void LockPlayerInputs()
    {
        if (PlayerHandlerData.MainPlayer != null)
        {
            PlayerHandlerData.MainPlayer.Inventory.Enable();
            PlayerHandlerData.MainPlayer.Interactions.Enable();
            PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = true;
            PlayerHandlerData.MainPlayer.EnableCameraRotationInput = true;
        }
    }

    public override void Exit(TutorialContext context)
    {
        _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;
        _choiceIntroduction.OnChoiceSelected -= Captain_OnFirstChoiceSelected;
        _captainGoodWayGuide.OnChoiceSelected -= CaptainGoodWayGuide_OnChoiceSelected;
        _captainGoodWayGuide.OnChoiceSelected -= CaptainBadWayGuide_OnChoiceSelected;
        _forcedChoice.OnTimerComplete -= TimerChoiceDuration_OnTimerComplete;
    }

}