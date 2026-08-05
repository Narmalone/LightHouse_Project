using LightHouse.Core.Audio;
using LightHouse.Core.Tutorial;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Step03_GiveDirection", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step03_GiveDirection")]
public class Step03_GiveDirection : TutorialStep
{
    [SerializeField] private LocalizedDialogueAudio _captainBadWayGuide; //première erreur de direction
    [SerializeField] private LocalizedDialogueAudio _captainBadWay; //si une erreur de direction est faite
    [SerializeField] private LocalizedDialogueAudio _captainBadWayRepairEngine; //répare le moteur
    [SerializeField] private LocalizedDialogueAudio _captainBadWayEndSubTutorial; //retire argent du salaire
    [SerializeField] private LocalizedDialogueAudio _captainGoodWayGuide; //première bonne direction
    [SerializeField] private LocalizedDialogueAudio _captainGoodWay; //deuxième bonne direction

    public override void Enter(TutorialContext context)
    {
        base.Enter(context);

        context.TalkieManager.OnChoiceSelected += OnChoiceSelected;
    }

    private void OnChoiceSelected(TalkieChoice choice)
    {

    }

    public override void Exit(TutorialContext context)
    {
        context.TalkieManager.OnChoiceSelected -= OnChoiceSelected;
    }
}
