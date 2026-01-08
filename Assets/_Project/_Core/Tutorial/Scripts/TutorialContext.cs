using LightHouse.Game.Buyoncies;
using LightHouse.Game.Talkie;
using LightHouse.Items.Interactable;
using LightHouse.Items.Inventory;
using LightHouse.Weather;
using UnityEngine;

namespace LightHouse.Game.Tutorial
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
