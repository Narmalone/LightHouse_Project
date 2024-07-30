using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    public ScenarioEvent evt;

    private void Awake()
    {
        evt.eventAction += () =>
        {
            //Debug.Log("cc");
        };
    }
}
