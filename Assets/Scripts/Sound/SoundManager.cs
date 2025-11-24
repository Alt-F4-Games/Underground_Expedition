/*
 * SoundManager
 * Centralized audio manager responsible for:
 *  - Playing music and sound effects by ID
 *  - Handling separate mixers for music and SFX
 *  - Persisting across scenes (DontDestroyOnLoad)
 *  - Storing a registry of audio clips defined in the Sound[] list
 *
 * Features:
 *  - Singleton pattern (SoundManager.Instance)
 *  - Adjustable global volumes (musicVolume / sfxVolume)
 *  - Automatic AudioSource creation for music and SFX
 *
 * Dependencies:
 *  - A Sound[] array populated with Sound objects (custom struct/class)
 *  - SoundType enum specifying Music or SFX
 *  - Audio clips assigned to each Sound entry
 */

using System;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Global Settings")]
    [Tooltip("Master volume for all music.")]
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Tooltip("Master volume for all sound effects.")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Sound List")]
    [Tooltip("List of all registered sounds accessible by ID.")]
    public Sound[] sounds;

    // Dedicated AudioSources for organized playback
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Singleton pattern: ensure only one SoundManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Music source (looping)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        // SFX source (non-looping unless sound specifies it)
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    // Plays a sound by its ID
    public void Play(string id)
    {
        Sound sound = sounds.FirstOrDefault(s => s.id == id);

        if (sound == null)
        {
            Debug.LogWarning($"[SoundManager] Sound not found: {id}");
            return;
        }

        // Select correct AudioSource based on sound type
        AudioSource targetSource =
            sound.type == SoundType.Music ? musicSource : sfxSource;

        // Configure playback
        targetSource.clip = sound.clip;
        targetSource.volume =
            sound.volume * (sound.type == SoundType.Music ? musicVolume : sfxVolume);

        targetSource.pitch = sound.pitch;
        targetSource.loop = sound.loop;

        targetSource.Play();
    }

    // Stops a sound only if it is currently playing on its corresponding channel
    public void Stop(string id)
    {
        Sound sound = sounds.FirstOrDefault(s => s.id == id);
        if (sound == null) return;

        AudioSource targetSource =
            sound.type == SoundType.Music ? musicSource : sfxSource;

        if (targetSource.isPlaying && targetSource.clip == sound.clip)
        {
            targetSource.Stop();
        }
    }

    // Changes the global volume of either music or SFX
    public void SetVolume(SoundType type, float value)
    {
        if (type == SoundType.Music)
        {
            musicVolume = value;

            // Apply volume ONLY to the music source
            if (musicSource.clip != null)
                musicSource.volume = musicSource.clip != null
                    ? musicSource.volume = value
                    : musicSource.volume;
        }
        else
        {
            sfxVolume = value;

            // Apply volume ONLY to the SFX source
            if (sfxSource.clip != null)
                sfxSource.volume = value;
        }
    }
}
