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

        public override void PlayLand() { PlayOneShot(AudioKeys.PlayerJumpOff); }
        public override void PlayAttack() { PlayOneShot(AudioKeys.PlayerAttack); }
        public override void PlayMissAttack() { PlayOneShot(AudioKeys.PlayerMissAttack); }
        public override void PlayDamaged() { PlayOneShot(AudioKeys.PlayerDamaged); }
        public override void PlayDeath() { PlayOneShot(AudioKeys.PlayerDeath); }
        public override void PlayEmpoweredAttack() { PlayOneShot(AudioKeys.PlayerAbilityAttack); }
        public override void PlayAttackAOE() { PlayOneShot(AudioKeys.PlayerAbilityAoe); }
    }
}