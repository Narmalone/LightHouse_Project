using LightHouse.Handlers;
using UnityEngine;

public class RainTesting : MonoBehaviour
{
    public AudioCue windCue;
    void Start()
    {
        ServiceLocator.Audio.PlayAt(windCue, PlayerHandlerData.MainPlayer.Character.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
