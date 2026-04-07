using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using Network.Enemies;
using UnityEngine.AI;

public enum SpawnType
{
    Enemy,
    Pickup,
    Destructible
}

public class SpawnPoints : NetworkBehaviour
{
    [SerializeField] private SpawnType spawnType;

    [SerializeField] private NetworkPrefabRef prefab;

    [SerializeField] private float _offsetSpawn;
    
    [Header("Enemy / Destructible")]
    [SerializeField] private int _enemyAmount;
    [Header("Enemy Specific")]
    [Tooltip("The path that instantiated enemies will follow.")]
    [SerializeField] private NetworkPatrolPath _patrolPath;
    
    [Header("Pickup")]
    [SerializeField] private int _pickupsAmount;
    [SerializeField] private int pickupId;
    [SerializeField] private int _amount;
    

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        Spawn();
    }

    private void Spawn()
    {
        switch (spawnType)
        {
            case SpawnType.Enemy:
            case SpawnType.Destructible:
                // TODO: separar la logica del Case spawneo de Desctructibles y de Enemigos 
                if (!prefab.IsValid)
                {
                    Debug.LogWarning($"No prefab in {name}");
                    return;
                }

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

            case SpawnType.Pickup:

                for (int i = 0; i < _pickupsAmount; i++)
                {
                    Vector3 offset = new Vector3(Random.Range(-_offsetSpawn, _offsetSpawn), 0, Random.Range(-_offsetSpawn, _offsetSpawn));
                    var obj = Runner.Spawn(prefab, transform.position + offset, transform.rotation);
                                                                                                  
                      if (obj.TryGetComponent(out NetworkWorldItem item))
                      {
                          item.Init(pickupId, _amount);
                      }
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