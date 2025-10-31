using LightHouse.Audio;

namespace LightHouse.Game.Talkie
{
    public interface ITalkieService
    {
        void Enqueue(LocalizedDialogueAudio dialogueId);
    }
}
