using Audio.Core;
using Fusion;
using Network.Enemies;

namespace Audio.Enemies
{
    public class AhPuchAudio : EnemyAudioBase
    {
        
        private NetworkAhPuchController _controller;
        
        private void Awake()
        {
            _controller = GetComponent<NetworkAhPuchController>();
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

                    StartLoop(AudioKeys.BossWalk);

                    break;
                
                case NetworkEnemyState.Attacking:
                    
                    StopLoop();
                    
                    break;
                
                default:

                    StopLoop();

                    break;
            }
        }
        
        public override void PlaySpawn()
        {
            if (HasStateAuthority)
            {
                RPC_PlaySpawn();
            }
        }
        
        public override void PlayEvoke()
        {
            if (HasStateAuthority)
            {
                RPC_PlayEvoke();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlaySpawn()
        {
            StopLoop();
            PlayOneShot(AudioKeys.BossSpawn);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayEvoke()
        {
            PlayOneShot(AudioKeys.BossEvoke);
        }
    }
}