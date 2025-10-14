using LightHouse.Handlers;
using System.Collections;
using UnityEngine;

public class UnityTest : MonoBehaviour
{
    public AudioCue cue;
    public AudioCue musicTest;
    public float delay = 3f; // délai entre chaque son

    void Start()
    {
        StartCoroutine(TestRoutine());
        ServiceLocator.Audio.PlayAt(musicTest, Vector3.zero);
    }

    private IEnumerator TestRoutine()
    {
        // tant que le jeu est actif et que ce script est activé
        while (isActiveAndEnabled)
        {
            // jouer le son
            if(PlayerHandlerData.MainPlayer != null)
                ServiceLocator.Audio.PlayAt(cue, PlayerHandlerData.MainPlayer.Character.transform.position);

            // attendre le délai avant de rejouer
            yield return new WaitForSeconds(delay);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
