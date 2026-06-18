using Audio.Core;
using Fusion;
using UnityEngine;

namespace Audio.Player
{
    public class PlayerAudio : PlayerAudioBase
    {
        private readonly string[] _footsteps =
        {
            AudioKeys.PlayerFootstep1,
            AudioKeys.PlayerFootstep2,
            AudioKeys.PlayerFootstep3,
            AudioKeys.PlayerFootstep4,
            AudioKeys.PlayerFootstep5
        };

        public override void PlayFootstep()
        {
            int index = Random.Range(0, _footsteps.Length);

            PlayOneShot(_footsteps[index]);
        }

        public override void PlayDamage()
        {
            // futuro
        }

        public override void PlayAttack()
        {
            // futuro
        }

        public override void PlayDeath()
        {
            // futuro
        }
    }
}