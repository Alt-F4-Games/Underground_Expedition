using UnityEngine;

public enum SpawnType
{
    Enemy,
    Pickup,
    Destructible
}

public class SpawnPoints : MonoBehaviour
{
    public SpawnType spawnType;
    
    public bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
