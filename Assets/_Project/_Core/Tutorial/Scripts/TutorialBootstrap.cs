using LightHouse.Game.Buyoncies;
using LightHouse.Game.Talkie;
using LightHouse.Handlers;
using LightHouse.Items.Interactable;
using LightHouse.Items.Inventory;
using LightHouse.Weather;
using UnityEngine;

namespace LightHouse.Game.Tutorial
{
    public sealed class TutorialBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _skipTutorial = false;
        [SerializeField] private bool _forcePlayerSpawnOnIsland = false;
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
