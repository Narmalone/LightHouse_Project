using UnityEngine;

namespace LightHouse.Game.Talkie
{
    public class TalkieManager : MonoBehaviour
    {
        public TalkieSentence sentence;

        private void Awake()
        {
            sentence.Register();
        }

        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            sentence.Unregister();
        }
    }

}
