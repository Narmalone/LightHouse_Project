using LightHouse.Core.Audio;

namespace LightHouse.Features.Talkie
{
    public interface ITalkieService
    {
        void Enqueue(LocalizedDialogueAudio dialogueId);
    }
}
