using System.Linq;
using UnityEngine;

namespace LightHouse.Game.Talkie
{
    [CreateAssetMenu(fileName = "TalkieSequence_", menuName = "LightHouse/Talkie/New Sequence")]
    public class TalkieSequencePlayer : ScriptableObject, ITalkieSequence
    {
        [SerializeField] private TalkieSentence[] Sentences;
        private byte _currentIndex = 0;

        public bool HasNext => _currentIndex < Sentences.Length;

        public void PlayNext()
        {
            if (!HasNext) return;
            //Sentences[_currentIndex].
            _currentIndex++;
            Debug.Log("doit jouer le prochain");
        }

        public void StartSequence()
        {
            _currentIndex = 0;
            PlayNext();
        }

    }

}
