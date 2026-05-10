using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using System.Collections;
using UnityEngine;

public class ThunderEffect : MonoBehaviour
{
    public SO_AudioCue thunder;
    public ThunderSpawner spawner;

    public float speedOfSound = 343f;

    public void PlayThunder(Vector3 lightningPos)
    {
        StartCoroutine(PlayRoutine(lightningPos));
    }

    IEnumerator PlayRoutine(Vector3 lightningPos)
    {
        Vector3 listenerPos = Camera.main.transform.position;
        float dist = Vector3.Distance(listenerPos, lightningPos);

        float delay = dist / speedOfSound;

        yield return new WaitForSeconds(delay);

        Vector3 thunderPos = spawner != null
            ? spawner.GetThunderPosition(lightningPos)
            : lightningPos;

        ServiceLocator.Audio.PlayAt(thunder, thunderPos);
    }
}