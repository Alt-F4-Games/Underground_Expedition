using UnityEngine;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour
{
    [Header("Unique identifier of the spawn point")]
    public string spawnID;

    [Header("Optional Patrol Path for spawned enemies")]
    [Tooltip("The SpawnPoint itself will be the first waypoint automatically.")]
    public List<Transform> additionalWaypoints = new();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, spawnID);

        if (additionalWaypoints == null || additionalWaypoints.Count == 0)
            return;
        
        Gizmos.color = Color.yellow;
        Vector3 lastPos = transform.position;

        foreach (var wp in additionalWaypoints)
        {
            if (wp == null) continue;
            Gizmos.DrawLine(lastPos, wp.position);
            Gizmos.DrawSphere(wp.position, 0.2f);
            lastPos = wp.position;
        }
        
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.7f);
        Gizmos.DrawLine(lastPos, transform.position);
    }
#endif
}