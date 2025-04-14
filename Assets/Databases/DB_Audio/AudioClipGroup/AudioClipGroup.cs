using UnityEngine;

public abstract class AudioClipGroup : ScriptableObject
{
    public string displayName;
    public AudioClip[] clips;
    public float volume = 1f;
    public bool loop = false;
    public float _spatialBlend = 0.0f;

    public virtual AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;
        if(clips.Length == 1) return clips[0];  
        return clips[Random.Range(0, clips.Length)];
    }
}
