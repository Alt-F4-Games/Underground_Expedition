using Fusion;
using UnityEngine;

namespace Health
{
    public class NetworkDespawnOnDeath : NetworkHealthSystem
    {
        protected override void Death()
        {
            base.Death();

            if (!HasStateAuthority) return;

            Debug.Log($"[SERVER] Despawning {gameObject.name}");

            Runner.Despawn(Object);
        }
    }
}