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
        public List<SpawnPoints> spawnPoints;
        public int amount = 1;
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
        if (entry.prefab == null)
        {
            Debug.LogError($"[Spawner] Entry '{entry.name}' has NULL prefab!");
            return;
        }

        if (entry.spawnPoints == null || entry.spawnPoints.Count == 0)
        {
            Debug.LogError($"[Spawner] Entry '{entry.name}' has NO spawn points!");
            return;
        }

        Debug.Log($"[Spawner] Spawning '{entry.name}' → {entry.amount} per spawn point");

        foreach (var spawnPoint in entry.spawnPoints)
        {
            if (spawnPoint == null)
            {
                Debug.LogError($"[Spawner] NULL spawn point in entry '{entry.name}'");
                continue;
            }

            Debug.Log($"[Spawner] Using spawn point: {spawnPoint.name}");

            for (int i = 0; i < entry.amount; i++)
            {
                Vector3 spawnPos = spawnPoint.transform.position;

                var obj = Runner.Spawn(
                    entry.prefab,
                    spawnPos,
                    spawnPoint.transform.rotation
                );

                if (obj == null)
                {
                    Debug.LogError($"[Spawner] ❌ Spawn FAILED for {entry.name}");
                }
                else
                {
                    Debug.Log($"[Spawner] ✅ Spawned {entry.name} at {spawnPos}");
                }
            }
        }
    }
}
