using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Audio.Core
{
    public class AudioEmitter
    {
        private EventInstance _instance;

        public AudioEmitter(EventReference eventReference, Transform transform)
        {
            _instance = RuntimeManager.CreateInstance(eventReference);

            RuntimeManager.AttachInstanceToGameObject(_instance, transform, transform.GetComponent<Rigidbody>());
        }

        public void Play()
        {
            _instance.start();
        }

        public void Stop()
        {
            _instance.stop(STOP_MODE.ALLOWFADEOUT);
        }

        public bool IsPlaying()
        {
            _instance.getPlaybackState(out PLAYBACK_STATE state);

            return state == PLAYBACK_STATE.PLAYING;
        }

        public void Release()
        {
            _instance.stop(STOP_MODE.IMMEDIATE);
            _instance.release();
        }
    }
}