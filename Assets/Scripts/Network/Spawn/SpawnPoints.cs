using Fusion;
using UnityEngine;

public enum SpawnType
{
    Enemy,
    Pickup,
    Destructible
}

public class SpawnPoints : NetworkBehaviour
{
    public SpawnType spawnType;

    [Header("Enemy / Destructible")]
    public NetworkPrefabRef prefab;

    [Header("Pickup")]
    public int pickupId;
    public int amount;

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

                Runner.Spawn(prefab, transform.position, transform.rotation);
                break;

            case SpawnType.Pickup:
                var obj = Runner.Spawn(prefab, transform.position, transform.rotation);

                if (obj.TryGetComponent(out NetworkWorldItem item))
                {
                    item.Init(pickupId, amount);
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
