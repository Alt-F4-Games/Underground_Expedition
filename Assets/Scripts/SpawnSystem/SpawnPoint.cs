using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Identificador único del punto de spawn")]
    public string spawnID;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, spawnID);
    }
#endif
}