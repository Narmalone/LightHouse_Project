using LightHouse.Game.Buyoncies;
using LightHouse.Game.Talkie;
using LightHouse.Handlers;
using LightHouse.Weather;
using UnityEngine;

namespace LightHouse.Game.Tutorial
{
    public sealed class TutorialBootstrap : MonoBehaviour
    {
        [SerializeField] private TalkieServiceReference _talkieRef;
        [SerializeField] private TutorialFlow _flow;

        [Header("Refs")]
        [SerializeField] private BinocularItem _binocular;
        [SerializeField] private Transform _lighthouse;
        [SerializeField] private Transform _rockPoint;
        [SerializeField] private BuyoncyController _nearbyBuoy;
        [SerializeField] private WeatherTimeline _timeline;

        private void Start()
        {
            var ctx = new TutorialContext
            {
                Flow = _flow,
                Talkie = _talkieRef.Current,
                TalkieManager = _talkieRef.Current as TalkieManager,
                Binocular = _binocular,
                Lighthouse = _lighthouse,
                Rock = _rockPoint,
                NearbyBuoy = _nearbyBuoy,
                Timeline = _timeline,
                ViewTransform = PlayerHandlerData.MainPlayer?.PlayerCamera?.transform
            };

            _flow.Init(ctx);
        }
    }
}
