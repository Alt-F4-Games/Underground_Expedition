using Events;
using UnityEngine;

namespace Network.Spawn
{
    public class BossProximitySpawner : EnemyProximitySpawner
    {
        [Header("Event")]
        [SerializeField] private BoolEventChannel bossSpawnedEvent;

        protected override void TriggerSpawn()
        {
            base.TriggerSpawn();

            if (Object.HasStateAuthority)
            {
                bossSpawnedEvent.RaiseEvent(true);
            }
        }
    }
}