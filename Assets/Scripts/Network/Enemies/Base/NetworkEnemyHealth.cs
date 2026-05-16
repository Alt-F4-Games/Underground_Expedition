using Events;
using Fusion;
using Network.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Health
{
    public class NetworkEnemyHealth : NetworkDespawnOnDeath
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemySO enemyData;

        [Header("Rewards")]
        [SerializeField] private int expPerKill;

        private PlayerRef _lastDamager;
        
        public override void TakeDamage(int damage, PlayerRef playerRef)
        {
            _lastDamager = playerRef;

            base.TakeDamage(damage, playerRef);
        }

        protected override void Death()
        {
            base.Death();
            EnemyDiedEvent enemyDiedEvent = new EnemyDiedEvent
            {
                killer = _lastDamager,
                enemyId = enemyData.enemyId,
                exp = expPerKill
            };

            EventController.Instance.TriggerEvent(enemyDiedEvent);

        }
    }
}