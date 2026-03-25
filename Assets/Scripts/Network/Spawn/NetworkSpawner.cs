using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class NetworkSpawner : NetworkBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string name;
        public NetworkObject prefab;
        public SpawnType spawnType;
        public int amount;
    }

    [Header("Spawn Config")]
    [SerializeField] private List<SpawnEntry> _spawnEntries;

    
    
    public override void Spawned()
    {
    
        Debug.Log($"[Spawner] Spawned() called on {gameObject.name}");

        if (!HasStateAuthority)
        {
            Debug.LogWarning("[Spawner] No State Authority → aborting spawn");
            return;
        }

        Debug.Log("[Spawner] Has State Authority → spawning all entries");

        SpawnAll();

    }

    private void SpawnAll()
    {
        Debug.Log($"[Spawner] SpawnAll() → Entries count: {_spawnEntries.Count}");

        foreach (var entry in _spawnEntries)
        {
            Debug.Log($"[Spawner] Processing entry: {entry.name}");

            SpawnEntryObjects(entry);
        }
    }
    
    private void SpawnEntryObjects(SpawnEntry entry)
    {
        var allPoints = FindObjectsOfType<SpawnPoints>();

        var validPoints = new List<SpawnPoints>();

        foreach (var point in allPoints)
        {
            if (point.spawnType == entry.spawnType)
            {
                validPoints.Add(point);
            }
        }

        if (validPoints.Count == 0)
        {
            Debug.LogWarning($"[Spawner] No spawn points for type {entry.spawnType}");
            return;
        }

        foreach (var spawnPoint in validPoints)
        {
            for (int i = 0; i < entry.amount; i++)
            {
                Runner.Spawn(
                    entry.prefab,
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation
                );

                Debug.Log($"[Spawner] Spawned {entry.name} at {spawnPoint.transform.position}");
            }
        }
    }
}
