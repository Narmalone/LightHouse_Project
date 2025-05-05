using UnityEngine;


namespace LightHouse.Game.Talkie
{
    public interface ITalkieSequence
    {
        void StartSequence();
        bool HasNext { get; }
        void PlayNext();
    }

}
