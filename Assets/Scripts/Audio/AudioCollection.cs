using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioCollection", menuName = "Audio/Audio Collection")]
    public class AudioCollection : ScriptableObject
    {
        [SerializeField]
        private List<AudioEntry> entries = new();

        private Dictionary<string, EventReference> _cache;

        private void OnEnable()
        {
            _cache = null;
        }

        private void BuildCache()
        {
            if (_cache != null)
                return;

            _cache = new Dictionary<string, EventReference>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.id))
                    continue;

                if (_cache.ContainsKey(entry.id))
                {
                    Debug.LogWarning($"Duplicate Audio ID '{entry.id}' in {name}");
                    continue;
                }

                _cache.Add(entry.id, entry.eventReference);
            }
        }

        public bool TryGetAudio(string id, out EventReference eventReference)
        {
            BuildCache();

            return _cache.TryGetValue(id, out eventReference);
        }
    }
}