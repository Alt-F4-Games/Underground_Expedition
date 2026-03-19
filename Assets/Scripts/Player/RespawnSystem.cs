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

    private Dictionary<string, RespawnPoint> respawnPoints = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RegisterAllRespawnPoints();
        InitializeStartingRespawn();
    }

    private void RegisterAllRespawnPoints()
    {
        respawnPoints.Clear();
        RespawnPoint[] points = FindObjectsOfType<RespawnPoint>();

        foreach (var p in points)
        {
            if (!respawnPoints.ContainsKey(p.respawnID))
                respawnPoints.Add(p.respawnID, p);
            else
                Debug.LogWarning("Duplicate Respawn ID detected: " + p.respawnID);
        }
    }

    private void InitializeStartingRespawn()
    {
        if (!string.IsNullOrEmpty(currentRespawnID))
            return;

        if (!string.IsNullOrEmpty(defaultRespawnID) &&
            respawnPoints.ContainsKey(defaultRespawnID))
        {
            currentRespawnID = defaultRespawnID;
            return;
        }

        foreach (var kvp in respawnPoints)
        {
            currentRespawnID = kvp.Key;
            return;
        }

        currentRespawnID = null;
    }

    public void SetRespawnPoint(string id)
    {
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
        if (string.IsNullOrEmpty(currentRespawnID))
            return null;

        if (respawnPoints.TryGetValue(currentRespawnID, out RespawnPoint point))
            return point.transform;

        return null;
    }
}
