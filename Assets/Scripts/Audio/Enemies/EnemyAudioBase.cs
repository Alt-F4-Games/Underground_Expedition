using Audio.Core;
using Fusion;
using UnityEngine;

namespace Audio.Enemies
{
    public abstract class EnemyAudioBase : NetworkBehaviour, IEnemyAudio
    {
        [SerializeField] protected AudioCollection audioCollection;

        protected AudioEmitter movementEmitter;

        protected void PlayOneShot(string audioKey)
        {
            if (!audioCollection.TryGetAudio(audioKey, out var sound))
                return;

            AudioManager.Instance.PlayOneShot(sound, transform.position);
        }

        protected void StartLoop(string audioKey)
        {
            if (movementEmitter != null && movementEmitter.IsPlaying())
                return;

            if (!audioCollection.TryGetAudio(audioKey, out var sound))
                return;

            movementEmitter = AudioManager.Instance.CreateEmitter(sound, transform);

            movementEmitter.Play();
        }

        protected void StopLoop()
        {
            movementEmitter?.Stop();
        }

        protected virtual void OnDestroy()
        {
            movementEmitter?.Release();
        }
        
        public virtual void PlayDeath() { }

        public virtual void PlayDamage() { }

        public virtual void PlayExplosion() { }
    }
}