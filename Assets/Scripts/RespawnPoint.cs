/*
 * RespawnPoint
 * Represents a single respawn location in the scene.
 * Each point must have a unique string ID.
 * RespawnSystem uses these IDs to manage respawn logic.
 */

using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [Header("Unique Respawn Identifier")]
    public string respawnID; // This ID must be unique across all respawn points

    private void OnDrawGizmos()
    {
        // Draws a sphere in the editor to visually mark the respawn position
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}