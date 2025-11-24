/*
 * RespawnSystem
 * This script manages all RespawnPoints in the scene.
 * It registers them automatically, keeps track of the active respawn ID,
 * and provides access to the current respawn position.
 *
 * Dependencies:
 * - Requires RespawnPoint components placed in the scene.
 * - Other scripts can request the active respawn via GetCurrentRespawnTransform().
 */

using System.Collections.Generic;
using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    public static RespawnSystem Instance { get; private set; }

    [Header("Default Respawn ID (Optional)")]
    [Tooltip("If empty, the system will use the first RespawnPoint found in the scene.")]
    [SerializeField] private string defaultRespawnID = null;

    [Header("Active Respawn ID")]
    [SerializeField] private string currentRespawnID = null;

    // Stores all respawn points indexed by their unique ID
    private Dictionary<string, RespawnPoint> respawnPoints = new();

    private void Awake()
    {
        // Enforces singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RegisterAllRespawnPoints();   // Detects all RespawnPoint components in the scene
        InitializeStartingRespawn();  // Determines which respawn point to use first
    }

    private void RegisterAllRespawnPoints()
    {
        // Clears previous data to avoid duplicates
        respawnPoints.Clear();
        RespawnPoint[] points = FindObjectsOfType<RespawnPoint>();

        foreach (var p in points)
        {
            // Adds respawn points using their ID as dictionary key
            if (!respawnPoints.ContainsKey(p.respawnID))
                respawnPoints.Add(p.respawnID, p);
            else
                Debug.LogWarning("Duplicate Respawn ID detected: " + p.respawnID);
        }
    }

    private void InitializeStartingRespawn()
    {
        // If a starting respawn was already set, skip initialization
        if (!string.IsNullOrEmpty(currentRespawnID))
            return;

        // Use default respawn ID if provided and valid
        if (!string.IsNullOrEmpty(defaultRespawnID) &&
            respawnPoints.ContainsKey(defaultRespawnID))
        {
            currentRespawnID = defaultRespawnID;
            return;
        }

        // If no default ID exists, choose the first available one
        foreach (var kvp in respawnPoints)
        {
            currentRespawnID = kvp.Key;
            return;
        }

        // If no points exist at all
        currentRespawnID = null;
    }

    public void SetRespawnPoint(string id)
    {
        // Sets a new active respawn point if ID exists
        if (respawnPoints.ContainsKey(id))
        {
            currentRespawnID = id;
            Debug.Log("Active Respawn updated to ID: " + id);
        }
        else
        {
            Debug.LogWarning("Attempted to set invalid Respawn ID: " + id);
        }
    }

    public Transform GetCurrentRespawnTransform()
    {
        // Checks if an ID is assigned
        if (string.IsNullOrEmpty(currentRespawnID))
            return null;

        // Returns Transform of the active respawn point
        if (respawnPoints.TryGetValue(currentRespawnID, out RespawnPoint point))
            return point.transform;

        return null;
    }
}
