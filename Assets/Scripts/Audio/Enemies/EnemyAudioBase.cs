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
            if (movementEmitter != null)
            {
                if (movementEmitter.IsPlaying())
                    return;

                movementEmitter.Release();
                movementEmitter = null;
            }

            if (!audioCollection.TryGetAudio(audioKey, out var sound))
                return;

            movementEmitter = AudioManager.Instance.CreateEmitter(sound, transform);

            movementEmitter.Play();
        }

        protected void StopLoop()
        {
            if (movementEmitter == null)
                return;

            movementEmitter.Stop();
            movementEmitter.Release();
            movementEmitter = null;
        }

        protected virtual void OnDestroy()
        {
            movementEmitter?.Stop();
            movementEmitter?.Release();
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            StopLoop();

            movementEmitter?.Release();
            movementEmitter = null;

            base.Despawned(runner, hasState);
        }
        
        public virtual void PlayDeath() {}
        public virtual void PlayDamage() {}
        public virtual void PlayExplosion() {}
        public virtual void PlayAttack() {}
        public virtual void PlaySpawn() {}
        public virtual void PlayEvoke() {}
    }
}