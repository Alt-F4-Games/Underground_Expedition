using Audio.Core;
using Events;
using Fusion;
using Network.Enemies;
using Tools.EventSystem;
using UnityEngine;

namespace Audio
{
    public class RatAudio : MonoBehaviour
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
        }
        
        private void OnDisable()
        {
            EventController.Instance.RemoveListener<EnemyDiedEvent>(OnRatDied);
            EventController.Instance.RemoveListener<EnemyAttackEvent>(OnRatAttack);

        }

        private void OnRatDied(EnemyDiedEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;

            if (_ratAudioCollection.TryGetAudio(AudioKeys.RatDeath, out var sound))
            {
                AudioManager.Instance.PlayOneShot(sound, transform.position);
            }
        }

        private void OnRatAttack(EnemyAttackEvent evt)
        {
            if (evt.enemyObject != _networkObject)
                return;
            
            if (_ratAudioCollection.TryGetAudio(AudioKeys.RatAttack01, out var sound))
            {
                AudioManager.Instance.PlayOneShot(sound, transform.position);
            }
        }
    }
}