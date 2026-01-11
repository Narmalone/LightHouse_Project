using UnityEngine;

namespace LightHouse.Features.Talkie
{
    [CreateAssetMenu(menuName = "LightHouse/Services/TalkieServiceReference")]
    public class TalkieServiceReference : ScriptableObject
    {
        public ITalkieService Current { get; private set; }
        public void Register(ITalkieService service) => Current = service;
        public void ResetService() => Current = null;
    }

}
