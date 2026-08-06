using LightHouse.Core.Audio;
using LightHouse.Core.Player;
using LightHouse.Core.Tutorial;
using UnityEngine;

[CreateAssetMenu(fileName = "Step02_PickUpBinnoculars", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step02_PickUpBinnoculars")]
public class Step02_PickUpBinnoculars : TutorialStep
{
    private TutorialContext _context;
    [SerializeField] private LocalizedDialogueAudio _captainInitialDialogue;
    [SerializeField] private LocalizedDialogueAudio _captainPickUpBinnocularsDialogue;
    [SerializeField] private LocalizedDialogueAudio _captainLeadWayDialogue;

    public override void Enter(TutorialContext context)
    {
        base.Enter(context);

        _context = context;

        context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

        context.TalkieManager.Enqueue(_captainInitialDialogue);
        context.TalkieManager.Enqueue(_captainPickUpBinnocularsDialogue);

        context.Binocular.ItemAddedToInventory += OnBinocularPickedUp;
        context.Binocular.OnItemUsed += OnBinocularUsed;
    }

    private void OnBinocularUsed()
    {
        LockPlayerInputs();
        ObjectiveManager.Current.CompleteObjective();
        IsComplete = true;
    }

    private void OnBinocularPickedUp()
    {
        _context.TalkieManager.Enqueue(_captainLeadWayDialogue);
        ObjectiveManager.Current.CompleteObjective();
    }

    private void OnDialogueFinished(LocalizedDialogueAudio audio)
    {
        if (audio == _captainPickUpBinnocularsDialogue)
        {
            ObjectiveManager.Current.SetObjective("Pick up the binoculars");
            _context.Binocular.SetPickable(true);
        }

        else if (audio == _captainLeadWayDialogue)
        {
            ObjectiveManager.Current.SetObjective("Get at the front and use the binoculars with left click");
        }
    }

    private void LockPlayerInputs()
    {
        if (PlayerHandlerData.MainPlayer != null)
        {
            PlayerHandlerData.MainPlayer.Inventory.Disable();
            PlayerHandlerData.MainPlayer.Interactions.Disable();
            PlayerHandlerData.MainPlayer.EnableAllCharacterInputs = false;
            PlayerHandlerData.MainPlayer.EnableCameraRotationInput = true;
        }
    }

    public override void Exit(TutorialContext context) 
    {
        context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;
        context.Binocular.ItemAddedToInventory -= OnBinocularPickedUp;
        context.Binocular.OnItemUsed -= OnBinocularUsed;
    }

}
