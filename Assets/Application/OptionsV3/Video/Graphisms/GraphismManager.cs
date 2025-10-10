using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GraphismManager : MonoBehaviour
{
    public VideoPresetSettings lowPreset;
    public VideoPresetSettings ultraPreset;

    private void Start()
    {
        lowPreset.ApplyToQualitySettings();
        StartCoroutine(RoutineUp());
        var rp = GraphicsSettings.currentRenderPipeline;
        Debug.Log(rp == null ? "[RP] Built-in (pas d'SRP)." : $"[RP] {rp.GetType().Name}");
    }

    private IEnumerator RoutineUp()
    {
        yield return new WaitForSeconds(5f);
        lowPreset.ApplyToQualitySettings();
        yield return new WaitForSeconds(5f);
        ultraPreset.ApplyToQualitySettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            lowPreset.ApplyToQualitySettings();
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ultraPreset.ApplyToQualitySettings();
        }
    }
}
