using LightHouse.Core.Audio;
using LightHouse.Features.TerrainSurface;
using System;
using TMPro.EditorUtilities;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step05_GetOnTheDeck", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step05_GetOnTheDeck")]
    public class Step05_GetOnTheDeck : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _arrivingSoon;
        [SerializeField] private LocalizedDialogueAudio _onDock;
        [SerializeField] private LocalizedDialogueAudio _onDepart;

        private Trigger _trigger;
        private TutorialContext _context;

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            _context.TalkieManager.Enqueue(_arrivingSoon);

            SubscribeEvents();
        }

        private void TutoBoat_OnPathCompleted()
        {
            ObjectiveManager.Current.SetObjective("Get on the dock.");

            _context.TalkieManager.Enqueue(_onDock);

            Collider collider = _context.RightMiddleBoatCollider;

            collider.isTrigger = true;

            _trigger = collider.GetComponent<Trigger>();
            _trigger.OnEntered += OnPlayerEntered;
        }

        private void OnPlayerEntered(Collider collider)
        {
            if (!collider.CompareTag("Player"))
                return;

            ObjectiveManager.Current.CompleteObjective();

            _context.TalkieManager.Enqueue(_onDepart);

            _trigger.OnEntered -= OnPlayerEntered;
            IsComplete = true;
        }

        public override void Exit(TutorialContext context)
        {
            UnsubscribeEvents();
        }
        private void SubscribeEvents()
        {
            _context.TutoBoat.OnPathCompleted += TutoBoat_OnPathCompleted;
        }

        private void UnsubscribeEvents()
        {
            _context.TutoBoat.OnPathCompleted -= TutoBoat_OnPathCompleted;
        }

    }
}