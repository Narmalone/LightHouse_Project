using LightHouse.Audio;
using LightHouse.Localization;
using UnityEngine;

namespace LightHouse.Game.Talkie
{
    public class TalkieManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private LocalizedStringDatabase_Talkie_Sentences _talkieSentencesDB;
        [SerializeField] private LocalizedDatabase_Talkie_Audio _talkieAudioDB;

    }

}
