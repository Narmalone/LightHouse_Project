using LightHouse.Core.Audio;

namespace LightHouse.Core.Services
{
    public static class ServiceLocator
    {
        private static IAudioPlayer _audio;

        public static IAudioPlayer Audio
        {
            get
            {
                if (_audio == null)
                {
                    UnityEngine.Debug.LogError("AudioService not registered!");
                }
                return _audio;
            }
        }

        public static void SetAudioPlayer(IAudioPlayer audioPlayer)
        {
            _audio = audioPlayer;
        }
    }
}