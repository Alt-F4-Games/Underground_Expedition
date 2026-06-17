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
        private NetworkObject _networkObject;
        private NetworkRatController _controller;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
            _controller = GetComponent<NetworkRatController>();
        }

        private void OnEnable()
        {
            EventController.Instance.AddListener<EnemyDiedEvent>(OnRatDied);
            EventController.Instance.AddListener<EnemyAttackEvent>(OnRatAttack);
            EventController.Instance.AddListener<EnemyTakeDamageEvent>(OnRatDamaged);
            _controller.OnEnemyStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            EventController.Instance.RemoveListener<EnemyDiedEvent>(OnRatDied);
            EventController.Instance.RemoveListener<EnemyAttackEvent>(OnRatAttack);
            EventController.Instance.RemoveListener<EnemyTakeDamageEvent>(OnRatDamaged);
            if (_controller != null) { _controller.OnEnemyStateChanged -= HandleStateChanged; }
        }

        private void HandleStateChanged(NetworkEnemyState state)
        {
            switch (state)
            {
                case NetworkEnemyState.Patrolling:
                case NetworkEnemyState.Chasing:

                    StartLoop(
                        AudioKeys.RatWalk);

                    break;

                default:

                    StopLoop();

                    break;
            }
        }

        private void OnRatDied(EnemyDiedEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            RPC_PlayDeath();
        }

        private void OnRatAttack(EnemyAttackEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            RPC_PlayAttack();
        }

        private void OnRatDamaged(EnemyTakeDamageEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            RPC_PlayDamage();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayDeath()
        {
            PlayOneShot(AudioKeys.RatDeath);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayAttack()
        {
            PlayOneShot(AudioKeys.RatAttack01);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayDamage()
        {
            PlayOneShot(AudioKeys.RatDamaged01);
        }
    }
}