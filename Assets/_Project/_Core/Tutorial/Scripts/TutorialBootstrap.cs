using LightHouse.Core.Player;
using LightHouse.Core.Tutorial.Boat;
using LightHouse.Features.Buyoncies;
using LightHouse.Features.Items.Interactable;
using LightHouse.Features.Items.Interactable.Bag;
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
        [SerializeField] private BinocularItem _binocular;
        [SerializeField] private Hammer _hammer;
        [SerializeField] private Transform _lighthouse;
        [SerializeField] private Transform _rockPoint;
        [SerializeField] private BuyoncyController _nearbyBuoy;
        [SerializeField] private IDUseItemTracker _pipe;
        [SerializeField] private BagItem _bag;
        [SerializeField] private TutorialBoat _tutoBoat;

        [SerializeField] private WeatherTimeline _timeline;

        private void Start()
        {
            if (_skipTutorial) return;
            var ctx = new TutorialContext
            {
                Flow = _flow,
                Talkie = _talkieRef.Current,
                TalkieManager = _talkieRef.Current as TalkieManager,
                Binocular = _binocular,
                Hammer = _hammer,
                Lighthouse = _lighthouse,
                Rock = _rockPoint,
                Pipe = _pipe,
                NearbyBuoy = _nearbyBuoy,
                Bag = _bag,
                TutoBoat = _tutoBoat,
                Timeline = _timeline,
                ViewTransform = PlayerHandlerData.MainPlayer?.PlayerCamera?.transform
            };

            _flow.Init(ctx);
        }
    }
}
