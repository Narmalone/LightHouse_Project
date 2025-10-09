using LightHouse.Options.V3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VideoOptionsController : MonoBehaviour
{
    public List<IOption> VideosOptions = new List<IOption>();

    private void Awake()
    {
        VideosOptions = GetComponentsInChildren<IOption>().ToList();
    }

    public void ApplyAllSettings()
    {
        Debug.Log("apply");
        foreach (var option in VideosOptions)
        {
            option.Apply();
        }
    }

    public void RevertAllSettings()
    {
        foreach (var option in VideosOptions)
        {
            option.Revert();
        }
    }

    private void OnValidate()
    {
        VideosOptions = GetComponentsInChildren<IOption>().ToList();
    }
}
