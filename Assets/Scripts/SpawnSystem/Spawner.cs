using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [SerializeField] private SpawnDatabase database;
    private Dictionary<string, SpawnPoint> spawnPoints = new();

    private void Awake()
    {
        var points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
        {
            if (!spawnPoints.ContainsKey(p.spawnID))
                spawnPoints.Add(p.spawnID, p);
        }
    }

    public GameObject Spawn(string objectID, string spawnID)
    {
        var spawnable = database.GetSpawnableByID(objectID);
        if (spawnable == null)
        {
            Debug.LogWarning($"[Spawner] Object not found with ID: {objectID}");
            return null;
        }

        if (!spawnPoints.TryGetValue(spawnID, out var spawnPoint))
        {
            Debug.LogWarning($"[Spawner] Spawn point not found with ID: {spawnID}");
            return null;
        }

        GameObject obj = Instantiate(spawnable.prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        if (spawnable.lifeTime > 0)
            Destroy(obj, spawnable.lifeTime);

        return obj;
    }
}