using Audio.Core;
using Fusion;
using UnityEngine;

namespace Audio.Player
{
    public abstract class PlayerAudioBase : NetworkBehaviour, IPlayerAudio
    {
        [SerializeField] protected AudioCollection audioCollection;

        protected void PlayOneShot(string audioKey)
        {
            if (!audioCollection.TryGetAudio(audioKey, out var sound))
                return;

            AudioManager.Instance.PlayOneShot(sound, transform.position);
        }

        public virtual void PlayFootstep() { }
        public virtual void PlayAttack() { }
        public virtual void PlayDamage() { }
        public virtual void PlayDeath() { }
        public virtual void PlayJump() { }
        public virtual void PlayLand() { }
        public virtual void PlayInteract() { }
        public virtual void PlayUseItem() { }
        public virtual void PlaySkill1() { }
        public virtual void PlaySkill2() { }
    }
}