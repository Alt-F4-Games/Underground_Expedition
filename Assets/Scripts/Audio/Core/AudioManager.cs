using FMODUnity;
using UnityEngine;

namespace Audio.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayOneShot(EventReference eventReference, Vector3 position)
        {
            RuntimeManager.PlayOneShot(eventReference, position);
        }

        public void PlayOneShot(EventReference eventReference)
        {
            RuntimeManager.PlayOneShot(eventReference);
        }
    }
}