/*
    Spawner.cs
    Centralized spawning system responsible for creating objects and enemies
    based on object IDs and spawn point IDs.

    Behavior:
    - Registers all SpawnPoint components found in the scene.
    - Instantiates prefabs defined in the SpawnDatabase.
    - Auto-configures enemy stats when the spawned object is an EnemyAI.
    - Dynamically builds a PatrolPath when the SpawnPoint defines extra waypoints.

    Dependencies:
    - SpawnDatabase (lookup for spawnable definitions)
    - SpawnPoint (position and waypoint data)
    - SpawnableObject / SpawnableObjectEnemy (spawn configuration)
    - EnemyAI (optional auto-setup if the object is an enemy)
*/

using UnityEngine;
using System.Collections.Generic;
using Enemy;

public class Spawner : MonoBehaviour
{
    // List of spawnables defined by ID
    [SerializeField] private SpawnDatabase database;

    // Fast lookup table for spawn points using their string ID
    private Dictionary<string, SpawnPoint> spawnPoints = new();

    private void Awake()
    {
        // Detect every SpawnPoint in the scene and store them by ID
        var points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
        {
            if (!spawnPoints.ContainsKey(p.spawnID))
                spawnPoints.Add(p.spawnID, p);
        }
    }

    public GameObject Spawn(string objectID, string spawnID)
    {
        // Find the spawnable object definition by ID
        var spawnable = database.GetSpawnableByID(objectID);
        if (spawnable == null)
        {
            Debug.LogWarning($"[Spawner] Object not found with ID: {objectID}");
            return null;
        }

        // Find the corresponding spawn point
        if (!spawnPoints.TryGetValue(spawnID, out var spawnPoint))
        {
            Debug.LogWarning($"[Spawner] Spawn point not found with ID: {spawnID}");
            return null;
        }

        // Instantiate the prefab on the spawn point location
        GameObject obj = Instantiate(spawnable.prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        // Auto-configure enemy settings if the spawned object has an EnemyAI component
        if (obj.TryGetComponent(out EnemyAI enemy))
        {
            if (spawnable is SpawnableObjectEnemy enemyData)
            {
                // --- Detection overrides ---
                if (enemyData.viewDistance > 0)
                    enemy.viewDistance = enemyData.viewDistance;
                if (enemyData.viewAngle > 0)
                    enemy.viewAngle = enemyData.viewAngle;

                // --- Attack overrides ---
                if (enemyData.attackRange > 0)
                    enemy.attackRange = enemyData.attackRange;
                if (enemyData.attackDamage > 0)
                    enemy.attackDamage = enemyData.attackDamage;
                if (enemyData.attackCooldown > 0)
                    enemy.attackCooldown = enemyData.attackCooldown;

                // --- Patrol overrides ---
                if (enemyData.waypointTolerance > 0)
                    enemy.waypointTolerance = enemyData.waypointTolerance;
            }

            // Build a dynamic patrol path using SpawnPoint waypoints
            if (spawnPoint.additionalWaypoints.Count > 0)
            {
                GameObject pathGO = new GameObject($"{spawnID}_DynamicPath");
                pathGO.transform.SetParent(transform);
                var path = pathGO.AddComponent<PatrolPath>();

                // SpawnPoint is always the first waypoint
                path.Waypoints.Add(spawnPoint.transform);

                // Add the rest of the defined waypoints
                foreach (var wp in spawnPoint.additionalWaypoints)
                {
                    if (wp != null)
                        path.Waypoints.Add(wp);
                }

                enemy.patrolPath = path;
            }
        }

        // Destroy automatically if a lifetime was defined
        if (spawnable.lifeTime > 0)
            Destroy(obj, spawnable.lifeTime);

        return obj;
    }
}

