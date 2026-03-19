using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [Header("Unique Respawn Identifier")]
    public string respawnID;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}