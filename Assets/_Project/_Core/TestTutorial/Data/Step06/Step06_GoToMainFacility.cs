using LightHouse.Core.Audio;
using LightHouse.Core.Utilities;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step06_GoToMainFacility", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step06_GoToMainFacility")]

    public class Step06_GoToMainFacility : TutorialStep
    {
        [SerializeField] private LocalizedDialogueAudio _startWalk;
        [SerializeField] private LocalizedDialogueAudio _findMainFacility;

        private Trigger _trigger;
        private TutorialContext _context;

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            ObjectiveManager.Current.SetObjective("Walk trought the forest and find the Main Facility");

            _context.TalkieManager.Enqueue(_startWalk);

            //Debug.Log(_context.Step6TriggerEvent);

            //_context.Step6TriggerEvent.OnEntered += OnTriggerEntered;

            //Debug.Log("[Step06] Subscribed to trigger.");

            SubscribeEvents();
        }

        private void OnTriggerEntered(GameObject @object)
        {
            _context.TalkieManager.Enqueue(_findMainFacility);

            _context.Step6TriggerEvent.OnEntered -= OnTriggerEntered;
        }

        

        private void SubscribeEvents()
        {
            _context.TalkieManager.OnDialogueFinished += TalkieManager_OnDialogueFinished;
        }

        private void TalkieManager_OnDialogueFinished(LocalizedDialogueAudio obj)
        {

        }

        private void UnsubscribeEvents()
        {

        }

        public override void Tick(TutorialContext ctx, float dt)
        {
            base.Tick(ctx, dt);

            if (Input.GetKeyDown(KeyCode.V))
            {
                PlayEnd();
            }
        }

        private void PlayEnd()
        {
            _context.TalkieManager.Enqueue(_findMainFacility);
        }

        public override void Exit(TutorialContext context)
        {
            UnsubscribeEvents();
        }

    }
}