using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using Network.Enemies;

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
                if (!prefab.IsValid)
                {
                    Debug.LogWarning($"No prefab in {name}");
                    return;
                }

                for (int i = 0; i < _enemyAmount; i++)
                {
                    Vector3 offset = new Vector3(Random.Range(-_offsetSpawn, _offsetSpawn), 0, Random.Range(-_offsetSpawn, _offsetSpawn));
                    
                    Runner.Spawn(prefab, transform.position + offset, transform.rotation, null, (runner, obj) => 
                    {
                        if (_patrolPath != null && obj.TryGetComponent(out NetworkEnemyController enemy))
                        {
                            enemy.PatrolPath = _patrolPath;
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