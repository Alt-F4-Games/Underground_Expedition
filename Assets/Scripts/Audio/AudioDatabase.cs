using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Audio Database")]
    public class AudioDatabase : ScriptableObject
    {
        public AudioEntry[] entries;
    }
}