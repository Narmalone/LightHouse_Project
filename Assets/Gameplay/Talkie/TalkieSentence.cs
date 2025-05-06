using LightHouse.Audio;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Talkie
{
    [CreateAssetMenu(fileName = "Talkie_Sentence_", menuName = "LightHouse/Talkie/New Dialogue")]
    public class TalkieSentence : ScriptableObject
    {
        //TO DOO, ici on découpe les phrases pour afficher petit à petit les choses
        public LocalizedDialogueAudio[] AudioClips;
        public LocalizedString[] Sentences;
        public float Delay = 0.0f;
        public float AdditionnalTimeAtEndOfDialogue = 0.3f;
    }

}
