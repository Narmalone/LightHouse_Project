using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "Scenario/Event/CrabInvasion")]
public class CrabInvasionEvent : ScenarioEvent
{
    public int numberOfCrabs;
    public GameObject crabPrefab;
    public Vector3 spawnArea;

    public override void Play()
    {
        base.Play();
        
    }
}
