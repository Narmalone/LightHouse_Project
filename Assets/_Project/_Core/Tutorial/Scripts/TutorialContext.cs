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
    /// <summary>
    /// Contient toutes les références nécessaires au bon déroulement du tutoriel.
    /// Chaque step se lit dedans, s'abonne/désabonne proprement à ce dont elle a besoin.
    /// </summary>
    public sealed class TutorialContext
    {
        public TutorialFlow Flow;
        public ITalkieService Talkie;
        public TalkieManager TalkieManager;

        public BinocularItem Binocular;
        public Hammer Hammer;

        public Transform Lighthouse;
        public Transform Rock;
        public BuyoncyController NearbyBuoy;
        public IDUseItemTracker Pipe;
        public BagItem Bag;
        public TutorialBoat TutoBoat;

        public WeatherTimeline Timeline;

        public Transform ViewTransform; // camera transform (à refresh si besoin)
    }
}
