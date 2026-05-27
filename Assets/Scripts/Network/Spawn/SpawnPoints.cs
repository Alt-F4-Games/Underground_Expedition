using Fusion;
using Network.Enemies;
using Network.Inventory;
using Network.Items;
using UnityEngine;
using UnityEngine.AI;

namespace Network.Spawn
{
    public enum SpawnType
    {
        Enemy,
        Pickup,
        Destructible
    }

    public class SpawnPoints : NetworkBehaviour
    {
        [SerializeField] protected SpawnType spawnType; // Changed to protected for child access

        [SerializeField] protected NetworkPrefabRef prefab; // Changed to protected for child access

        [SerializeField] protected float _offsetSpawn; // Changed to protected for child access
    
        [Header("Enemy / Destructible")]
        [SerializeField] protected int _enemyAmount; // Changed to protected for child access
    
        [Header("Enemy Specific")]
        [Tooltip("The path that instantiated enemies will follow.")]
        [SerializeField] protected NetworkPatrolPath _patrolPath; // Changed to protected for child access
    
        [Header("Pickup")]
        [SerializeField] private int _pickupsAmount;
        [SerializeField] private string pickupId;
        [SerializeField] private int _amount;
    

        public override void Spawned()
        {
            if (!HasStateAuthority)
                return;

            Spawn();
        }

        // Changed to protected virtual to allow SummonPoint to trigger it manually
        protected virtual void Spawn()
        {
            switch (spawnType)
            {
                case SpawnType.Enemy:
                case SpawnType.Destructible:
                    if (!prefab.IsValid)
                    {
                        Debug.LogWarning($"No prefab in {name}");
                        return;
                    }
                    break;
            }

            switch (spawnType)
            {
                case SpawnType.Enemy:
                    for (int i = 0; i < _enemyAmount; i++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-_offsetSpawn, _offsetSpawn), 0, Random.Range(-_offsetSpawn, _offsetSpawn));
                        Vector3 desiredPosition = transform.position + offset;

                        // Validate the position on the NavMesh before instantiating
                        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                        {
                            desiredPosition = hit.position; // Snap to the actual NavMesh floor
                        }
                        else
                        {
                            Debug.LogWarning($"Spawn point {name} is too far from a valid NavMesh!");
                        }
                
                        Runner.Spawn(prefab, desiredPosition, transform.rotation, null, (runner, obj) => 
                        {
                            if (obj.TryGetComponent(out NetworkEnemyController enemy))
                            {
                                // Find the agent and enable it safely post-spawn
                                if (obj.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
                                {
                                    agent.enabled = true;
                                }
        
                                // Assign the patrol path
                                if (_patrolPath != null)
                                {
                                    enemy.PatrolPath = _patrolPath;
                                }
                            }
                        });
                    }
                    break;

                case SpawnType.Destructible:
                    for (int i = 0; i < _enemyAmount; i++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-_offsetSpawn, _offsetSpawn), 0, Random.Range(-_offsetSpawn, _offsetSpawn));
                        Runner.Spawn(prefab, transform.position + offset, transform.rotation);
                    }
                    break;

                case SpawnType.Pickup:
                    NetworkPrefabRef globalWorldItemPrefab = ItemDatabase.Instance.WorldItemPrefab;
                    if (!globalWorldItemPrefab.IsValid)
                    {
                        Debug.LogError($"[SpawnPoints] WorldItemPrefab no configurado en la ItemDatabase.");
                        return;
                    }

                    int networkId = ItemDatabase.Instance.GetNetworkId(pickupId);
                    if (networkId <= 0)
                    {
                        Debug.LogWarning($"[SpawnPoints] El ID '{pickupId}' no es válido en la ItemDatabase.");
                        return;
                    }

                    for (int i = 0; i < _pickupsAmount; i++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-_offsetSpawn, _offsetSpawn), 0, Random.Range(-_offsetSpawn, _offsetSpawn));
                        Vector3 spawnPos = transform.position + offset;

                        Runner.Spawn(globalWorldItemPrefab, spawnPos, transform.rotation, null, (runner, obj) => 
                        {
                            if (obj.TryGetComponent(out NetworkWorldItem item))
                            {
                                item.Init(networkId, _amount);
                            }
                        });
                    }
                    break;
            }
        }
    
        public bool drawGizmos = true;

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }
}