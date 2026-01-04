using LightHouse.EditorTools.SuperGameManager;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "LightHouse/Settings/New Audio Config Settings")]
public class AudioSettingsConfig : ScriptableObject
{
    [Range(0, 1f)]
    [SgmExpose(label: "Music Base Volume")]
    public float MusicBaseVolume = 0.5f;

    [Range(0, 1f)]
    [SgmExpose(label: "SFX Base Volume")]
    public float SFXBaseVolume = 0.5f;

    [Range(0, 1f)]
    [SgmExpose(label: "Dialogs Base Volume")]
    public float DialogsBaseVolume = 0.5f;

}
