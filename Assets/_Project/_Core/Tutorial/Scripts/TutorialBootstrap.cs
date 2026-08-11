using Cinemachine;
using LightHouse.Core.Player;
using LightHouse.Features.Boats;
using LightHouse.Features.Items.Inventory.Binoculars;
using LightHouse.Features.Items.Inventory.Hammer;
using LightHouse.Features.Talkie;
using LightHouse.Features.Weather;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    public sealed class TutorialBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _skipTutorial = false;
        [SerializeField] private TalkieServiceReference _talkieRef;
        [SerializeField] private TutorialFlow _flow;

        [Header("Refs")]
        [SerializeField] private Collider _rightMiddleBoatCollider;
        [SerializeField] private Collider _onDockCollider;
        [SerializeField] private CinemachineVirtualCamera _wakeUpCamera;
        [SerializeField] private TutorialChoiceBoat _tutoBoat;
        [SerializeField] private BinocularItem _binocularItem;
        [SerializeField] private Hammer _hammer;

        [SerializeField] private WeatherTimeline _timeline;

        private void Start()
        {

            if (_skipTutorial)
            {
                SkipTutorial();
            }
            else
            {
                InitializeTutorial();
            }
            
        }

        private void InitializeTutorial()
        {
            var ctx = new TutorialContext
            {
                Binocular = _binocularItem,
                Flow = _flow,
                Talkie = _talkieRef.Current,
                TalkieManager = _talkieRef.Current as TalkieManager,
                WakeUpCam = _wakeUpCamera,
                RightMiddleBoatCollider = _rightMiddleBoatCollider,
                Hammer = _hammer,
                TutoBoat = _tutoBoat,
                Timeline = _timeline,
                ViewTransform = PlayerHandlerData.MainPlayer?.PlayerCamera?.transform,
            };

            _tutoBoat.InitializeOnPath();
            _flow.Init(ctx);
        }

        private void SkipTutorial()
        {
            DisableAll();

            if (PreAlphaLevel.Current.TutorialSkipPos != null && PlayerHandlerData.MainPlayer != null)
            {
                Debug.Log("Skipping");
                PlayerHandlerData.MainPlayer.Character.ForceLookRotation(PreAlphaLevel.Current.TutorialSkipPos.rotation);

                PlayerHandlerData.MainPlayer.Character.SetPositionAndRotation(
                    PreAlphaLevel.Current.TutorialSkipPos.position,
                    PreAlphaLevel.Current.TutorialSkipPos.rotation,
                    true);

                PlayerHandlerData.MainPlayer.PlayerCamera.SetRotation(
                    PreAlphaLevel.Current.TutorialSkipPos.rotation);
            }
        }

        private void DisableAll()
        {
            _wakeUpCamera.Priority = -1;

            _tutoBoat.gameObject.SetActive(false);
            _tutoBoat.enabled = false;
            _binocularItem.gameObject.SetActive(false);
            _hammer.gameObject.SetActive(false);
        }
    }
}
