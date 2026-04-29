using Events;
using Fusion;
using UnityEngine;

namespace Health
{
    public class NetworkEnemyHealth : NetworkDespawnOnDeath
    {
        private EnemyDiedEvent _enemyDiedEventEvent = new ();

        public override void TakeDamage(int damage, PlayerRef playerRef)
        {
            base.TakeDamage(damage, playerRef);
            _enemyDiedEventEvent.exp = 10;
            _enemyDiedEventEvent.killer = playerRef;
            EventController.Instance.TriggerEvent(_enemyDiedEventEvent);
        }

        protected override void Death()
        {
            // Execute base logic (IsAlive = false, etc.)
            base.Death();
            Debug.Log($"[ENEMY] {gameObject.name} has died.");
        }
    }
}