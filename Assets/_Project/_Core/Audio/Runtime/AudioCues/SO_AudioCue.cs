using System;
using UnityEngine;
using UnityEngine.Audio;

namespace LightHouse.Core.Audio
{
    public enum AudioCategory { Music, Ambience, SFX, UI, Voice }
    public enum Spatialization { TwoD, ThreeD }

    /// <summary>
    /// Base class for all audio in the game.
    /// It represents a settings container for a sound effect, music track, ambience, etc. It can contain multiple variants (clips + settings)
    /// to allow for randomization and variation.       
    /// </summary>
    [CreateAssetMenu(fileName = "SO_SFX_", menuName = GlobalAssetsMenuPaths.AudioAssetsMenuPath + "New Cue")]
    public class SO_AudioCue : ScriptableObject
    {
        [Obsolete("Id is deprecated and will be removed in future versions by using a GUID.")]
        public string Id; 
        [field :SerializeField, Obsolete("Used by the registry but obsolete for now")] public AudioCategory Category { get; }

        /// <summary>
        /// The mixer group this sound should be routed through.
        /// </summary>
        public AudioMixerGroup MixerGroup;

        /// <summary>
        /// This is the possible group of clips that can be played for this cue. 
        /// The system will randomly pick one of the variants when the cue is played, allowing for variation in the sound. 
        /// Each variant can have its own clip and weight for random selection.
        /// </summary>
        public CueVariant[] Variants;

        /// <summary>
        /// If the sound should be looped when played. This is useful for music and ambience cues, but usually not for SFX.
        /// </summary>
        public bool Loop;

        /// <summary>
        /// The rolloff mode determines how the sound attenuates with distance when spatialized in 3D.
        /// </summary>
        public AudioRolloffMode Rolloff = AudioRolloffMode.Logarithmic;

        /// <summary>
        /// The Min Distance is the distance from the listener at which the sound will be heard at its maximum volume.
        /// </summary>
        public float MinDistance = 1f;

        /// <summary>
        /// The Max Distance is the distance from the listener at which the sound will be heard at its minimum volume.
        /// </summary>
        public float MaxDistance = 50f;

        /// <summary>
        /// The base volume of the sound when played. 
        /// This can be modified by random variation and by the distance from the listener when spatialized in 3D.
        /// </summary>
        public float Volume = 1f;

        /// <summary>
        /// The pitch of the sound when played. 
        /// This can be modified by random variation and can be used to create interesting effects, especially for SFX.
        /// </summary>
        public float Pitch = 1f;

        /// <summary>
        /// The pitch variation when the cue is played. 
        /// The system will randomly vary the pitch of the sound by a value between -RandomPitch and +RandomPitch, 
        /// allowing for more natural and less repetitive sounds.
        /// </summary>
        public float RandomPitch = 0.0f;

        /// <summary>
        /// The volume variation when the cue is played. 
        /// The system will randomly vary the volume of the sound by a value between -RandomVolume and +RandomVolume,
        /// allowing for more natural and less repetitive sounds.
        /// </summary>
        public float RandomVolume = 0.0f;

        /// <summary>
        /// The maximum number of simultaneous voices that can be played for this cue.
        /// If set to 0, there is no limit and the bus-level limiter will take over.
        /// </summary>
        public int MaxSimultaneousVoices = 0;

        /// <summary>
        /// The spatialization mode of the sound. 
        /// if set to TwoD, the sound will not be affected by the position of the listener 
        /// and will be heard at the same volume and pan regardless of distance and direction.
        /// </summary>
        public Spatialization Spatial = Spatialization.ThreeD;

        /// <summary>
        /// The stereo pan of the sound when played. 
        /// This is only used for 2D sounds and allows to pan the sound left or right in the stereo field.
        /// </summary>
        public float StereoPan = 0f;

        /// <summary>
        /// Bypass effects applied to the listener, such as reverb zones and listener effects.
        /// </summary>
        public bool BypassListenerFx = false;

        /// <summary>
        /// Bypass effects applied to the reverb zones, 
        /// allowing the sound to ignore reverb zones and be heard without reverb even when inside a reverb zone.
        /// </summary>
        public bool BypassReverbZones = false;

        /// <summary>
        /// Doppler level of the sound when played. 
        /// This determines how much the pitch of the sound will be affected by the relative velocity
        /// between the sound source and the listener.
        /// </summary>
        [Tooltip("When 2D set to 0")] public float DopplerLevel = 1f;
    }

    /// <summary>
    /// The wrapper for a single variant of a sound, containing the audio clip and its weight for random selection.
    /// </summary>
    [System.Serializable]
    public class CueVariant
    {
        public AudioClip Clip;
        [Range(0, 1)] public float Weight = 1f; 
    }


}
