using Audio.Core;
using Events;
using Fusion;
using Network.Enemies;
using Tools.EventSystem;
using UnityEngine;

namespace Audio.Enemies
{
    public class SkullAudio : EnemyAudioBase
    {
        private NetworkSkullController _controller;
        
        private void Awake()
        {
            _controller = GetComponent<NetworkSkullController>();
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

                    StartLoop(AudioKeys.SkullWalk);

                    break;

                case NetworkEnemyState.Charging:

                    StopLoop();

                    PlayOneShot(AudioKeys.SkullCharge);

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

        public override void PlayExplosion()
        {
            if (HasStateAuthority)
            {
                RPC_PlayExplosion();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_PlayDeath()
        {
            StopLoop();
            PlayOneShot(AudioKeys.SkullDeath);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayDamage()
        {
            PlayOneShot(AudioKeys.SkullDamaged);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_PlayExplosion()
        {
            StopLoop();
            PlayOneShot(AudioKeys.SkullExplotion);
        }
    }
}