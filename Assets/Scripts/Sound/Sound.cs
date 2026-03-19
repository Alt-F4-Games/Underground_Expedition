using UnityEngine;

public enum SoundType { Music, SFX }

[CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
public class Sound : ScriptableObject
{
    [Header("Identificación")]
    public string id; // Ej: "button_click", "enemy_death"

    [Header("Configuración")]
    public AudioClip clip;
    public SoundType type = SoundType.SFX;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
}