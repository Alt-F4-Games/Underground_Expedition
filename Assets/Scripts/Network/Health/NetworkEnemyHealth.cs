using Fusion;
using UnityEngine;

namespace Health
{
    public class NetworEnemyHealth : NetworkDespawnOnDeath
    {

        protected override void Death()
        {
            // Execute base logic (IsAlive = false, etc.)
            base.Death();

            Debug.Log($"[ENEMY] {gameObject.name} has died.");
        }
    }
}