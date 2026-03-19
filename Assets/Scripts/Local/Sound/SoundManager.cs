using System;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Configuración Global")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Lista de Sonidos")]
    public Sound[] sounds;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        sfxSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void Play(string id)
    {
        Sound sound = sounds.FirstOrDefault(s => s.id == id);
        if (sound == null)
        {
            Debug.LogWarning($"[SoundManager] Sonido no encontrado: {id}");
            return;
        }

        AudioSource targetSource = sound.type == SoundType.Music ? musicSource : sfxSource;

        targetSource.clip = sound.clip;
        targetSource.volume = sound.volume * (sound.type == SoundType.Music ? musicVolume : sfxVolume);
        targetSource.pitch = sound.pitch;
        targetSource.loop = sound.loop;
        targetSource.Play();
    }
    
    public void Stop(string id)
    {
        Sound sound = sounds.FirstOrDefault(s => s.id == id);
        if (sound == null) return;

        AudioSource targetSource = sound.type == SoundType.Music ? musicSource : sfxSource;
        if (targetSource.isPlaying && targetSource.clip == sound.clip)
        {
            targetSource.Stop();
        }
    }
    
    public void SetVolume(SoundType type, float value)
    {
        if (type == SoundType.Music)
        {
            musicVolume = value;
            musicSource.volume = musicSource.volume * musicVolume;
        }
        else
        {
            sfxVolume = value;
            sfxSource.volume = sfxSource.volume * sfxVolume;
        }
    }
}