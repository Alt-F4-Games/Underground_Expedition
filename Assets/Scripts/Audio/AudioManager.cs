using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Database")]
        [SerializeField] private AudioDatabase database;

        private readonly Dictionary<AudioEvent, EventReference> _events = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildDatabase();
        }

        private void BuildDatabase()
        {
            _events.Clear();

            foreach (AudioEntry entry in database.entries)
            {
                if (_events.ContainsKey(entry.audioEvent))
                {
                    Debug.LogWarning($"Duplicate AudioEvent found: {entry.audioEvent}");
                    continue;
                }

                _events.Add(entry.audioEvent, entry.eventReference);
            }
        }

        public void PlayOneShot(AudioEvent audioEvent)
        {
            if (!_events.TryGetValue(audioEvent, out var eventRef))
            {
                Debug.LogWarning($"Audio event not found: {audioEvent}");
                return;
            }

            RuntimeManager.PlayOneShot(eventRef);
        }

        public void PlayOneShot(AudioEvent audioEvent, Vector3 worldPosition)
        {
            if (!_events.TryGetValue(audioEvent, out var eventRef))
            {
                Debug.LogWarning($"Audio event not found: {audioEvent}");
                return;
            }

            RuntimeManager.PlayOneShot(eventRef, worldPosition);
        }

        public EventReference GetEventReference(AudioEvent audioEvent)
        {
            if (_events.TryGetValue(audioEvent, out var eventRef))
                return eventRef;

            Debug.LogWarning($"Audio event not found: {audioEvent}");

            return default;
        }
    }
}