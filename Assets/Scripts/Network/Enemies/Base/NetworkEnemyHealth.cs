using Events;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Health
{
    public class NetworkEnemyHealth : NetworkDespawnOnDeath
    {
        [SerializeField] private int _expPerKill;
        
        private EnemyDiedEvent _enemyDiedEventEvent = new ();

        public override void TakeDamage(int damage, PlayerRef playerRef)
        {
            base.TakeDamage(damage, playerRef);
            _enemyDiedEventEvent.killer = playerRef;
        }

        protected override void Death()
        {
            // Execute base logic (IsAlive = false, etc.)
            base.Death();
            _enemyDiedEventEvent.exp = _expPerKill;
            EventController.Instance.TriggerEvent(_enemyDiedEventEvent);
            Debug.Log($"[ENEMY] {gameObject.name} has died.");
        }
    }
}