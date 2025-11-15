using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public string respawnID;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}