using Events;
using Fusion;
using Network.Enemies;
using Network.Quests;
using Network.Quests.Enums;
using Tools.EventSystem;
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
            
            EventController.Instance.TriggerEvent(new EnemyTakeDamageEvent {enemyObject = Object});
        }

        protected override void Death()
        {
            EnemyDiedEvent enemyDiedEvent = new EnemyDiedEvent
            {
                killer = _lastDamager,
                enemyId = enemyData.enemyId,
                exp = expPerKill,
                enemyObject = Object
            };
            
            EventController.Instance.TriggerEvent(enemyDiedEvent);
            
            base.Death();
            
            NetworkQuestManager.Local.RPC_ReportQuestEvent(
                (int)QuestObjectiveType.KillEnemy,
                enemyData.enemyId,
                1);
        }
    }
}