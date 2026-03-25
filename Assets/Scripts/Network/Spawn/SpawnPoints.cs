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

    public void Spawn()
    {
        switch (spawnType)
        {
            case SpawnType.Enemy:
            case SpawnType.Destructible:
                SpawnNetworkObject();
                break;

            case SpawnType.Pickup:
                SpawnPickup();
                break;
        }
    }

    private void SpawnNetworkObject()
    {
        if (prefab.IsValid == false)
        {
            Debug.LogWarning($"No prefab assigned in {name}");
            return;
        }

        if (!HasStateAuthority) ;

        Runner.Spawn(prefab, transform.position, transform.rotation);
    }

    private void SpawnPickup()
    {
        if (!HasStateAuthority) ;
        var obj = Runner.Spawn(prefab, transform.position, transform.rotation);
        if (obj.TryGetComponent(out NetworkWorldItem item))
            item.Init(pickupId, amount); 
    }
    
    public bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
