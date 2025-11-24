/*
 *  Sound ScriptableObject
 *  This ScriptableObject represents a single sound entry that the
 *  SoundManager can use. It stores metadata such as ID, volume,
 *  pitch, sound type (Music or SFX), looping, and the AudioClip.
 *
 *  Dependencies:
 *  - Used by SoundManager to look up sounds by ID.
 *  - Requires UnityEngine for AudioClip and ScriptableObject.
 * --------------------------------------------------------------
 */

using UnityEngine;

public enum SoundType { Music, SFX }

[CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
public class Sound : ScriptableObject
{
    [Header("Identification")]
    public string id;

    [Header("Configuration")]
    public AudioClip clip; // Audio file to play
    public SoundType type = SoundType.SFX; // Defines if this sound is music or a sound effect

    [Range(0f, 1f)]
    public float volume = 1f; // Default volume for this sound

    [Range(0.1f, 3f)]
    public float pitch = 1f; // Playback pitch (1 = normal)

    public bool loop = false; // Whether this sound should loop when played
}