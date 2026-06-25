using System.Collections;
using Events;
using Fusion;
using Network.Enemies;
using Network.Quests;
using Network.Quests.Enums;
using Tools.EventSystem;
using UnityEngine;
using Audio.Enemies;

namespace Health
{
    public class NetworkEnemyHealth : NetworkHealthSystem
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemySO enemyData;

        [Header("Rewards")]
        [SerializeField] private int expPerKill;

        [SerializeField] private float deathDespawnDelay = 0.35f;     
        private PlayerRef _lastDamager;
        private IEnemyAudio _enemyAudio;
         
        private void Awake()
        {
            _enemyAudio = GetComponent<IEnemyAudio>();
        }
        
        public override void TakeDamage(int damage, PlayerRef playerRef)
        {
            _lastDamager = playerRef;

            base.TakeDamage(damage, playerRef);
            
            _enemyAudio?.PlayDamage();
            
            EventController.Instance.TriggerEvent(new EnemyTakeDamageEvent {enemyObject = Object});
        }

        protected override void Death()
        {
            base.Death();
            
            _enemyAudio?.PlayDeath();
            
            EnemyDiedEvent enemyDiedEvent = new EnemyDiedEvent
            {
                killer = _lastDamager,
                enemyId = enemyData.enemyId,
                exp = expPerKill,
                enemyObject = Object
            };

            if (HasStateAuthority)
            {
                if (TryGetComponent(out NetworkEnemyController controller) && controller.StateMachine != null)
                    controller.StateMachine.ChangeState(controller.GetDeadState());
            }
            
            EventController.Instance.TriggerEvent(enemyDiedEvent);
            
            StartCoroutine(DeathRoutine());
            
            NetworkQuestManager.Local.RPC_ReportQuestEvent(
                (int)QuestObjectiveType.KillEnemy,
                enemyData.enemyId,
                1);
        }
        
        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(deathDespawnDelay);

            if (Object && Object.IsValid)
            {
                Runner.Despawn(Object);
            }
        }
    }
}