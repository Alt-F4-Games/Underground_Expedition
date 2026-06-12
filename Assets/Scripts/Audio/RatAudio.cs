using Audio.Core;
using Events;
using Fusion;
using Network.Enemies;
using Tools.EventSystem;
using UnityEngine;

namespace Audio
{
    public class RatAudio : NetworkBehaviour
    {
        [SerializeField] private AudioCollection _ratAudioCollection;
        
        private NetworkObject _networkObject;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        private void OnEnable()
        {
            EventController.Instance.AddListener<EnemyDiedEvent>(OnRatDied);
            EventController.Instance.AddListener<EnemyAttackEvent>(OnRatAttack);
            EventController.Instance.AddListener<EnemyTakeDamageEvent>(OnRatTakeDamage);
        }
        
        private void OnDisable()
        {
            EventController.Instance.RemoveListener<EnemyDiedEvent>(OnRatDied);
            EventController.Instance.RemoveListener<EnemyAttackEvent>(OnRatAttack);
            EventController.Instance.RemoveListener<EnemyTakeDamageEvent>(OnRatTakeDamage);
        }

        private void OnRatDied(EnemyDiedEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            RPC_PlayRatDeathSound();
        }

        private void OnRatAttack(EnemyAttackEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            RPC_PlayRatAttackSound();
        }

        public void OnRatTakeDamage(EnemyTakeDamageEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            RPC_PlayRatTakeDamageSound();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayRatDeathSound()
        {
            if (_ratAudioCollection.TryGetAudio(AudioKeys.RatDeath, out var sound))
            {
                AudioManager.Instance.PlayOneShot(sound, transform.position);
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayRatAttackSound()
        {
            if (_ratAudioCollection.TryGetAudio(AudioKeys.RatAttack01, out var sound))
            {
                AudioManager.Instance.PlayOneShot(sound, transform.position);
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayRatTakeDamageSound()
        {
            if (_ratAudioCollection.TryGetAudio(AudioKeys.RatDamaged01, out var sound))
            {
                AudioManager.Instance.PlayOneShot(sound, transform.position);
            }
        }
    }
}