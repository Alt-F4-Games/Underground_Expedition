using Audio.Core;
using Events;
using Fusion;
using Network.Enemies;
using Network.Enemies.Variants;
using Tools.EventSystem;

namespace Audio.Enemies
{
    public class RatAudio : EnemyAudioBase
    {
        private NetworkRatController _controller;

        private void Awake()
        {
            _controller = GetComponent<NetworkRatController>();
        }

        private void OnEnable()
        {
            _controller.OnEnemyStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (_controller != null) { _controller.OnEnemyStateChanged -= HandleStateChanged; }
        }
        
        public override void Spawned()
        {
            base.Spawned();

            HandleStateChanged(_controller.CurrentState);
        }

        private void HandleStateChanged(NetworkEnemyState state)
        {
            switch (state)
            {
                case NetworkEnemyState.Patrolling:
                case NetworkEnemyState.Chasing:

                    StartLoop(AudioKeys.RatWalk);

                    break;
                
                default:

                    StopLoop();

                    break;
            }
        }
        
        public override void PlayDeath()
        {
            if (HasStateAuthority)
            {
                RPC_PlayDeath();
            }
        }

        public override void PlayDamage()
        {
            if (HasStateAuthority)
            {
                RPC_PlayDamage();
            }
        }
        
        public override void PlayAttack()
        {
            if (HasStateAuthority)
            {
                RPC_PlayAttack();
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayDeath()
        {
            StopLoop();
            PlayOneShot(AudioKeys.RatDeath);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayDamage()
        {
            PlayOneShot(AudioKeys.RatDamaged01);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayAttack()
        {
            PlayOneShot(AudioKeys.RatAttack01);
        }
    }
}